using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using EventSphere.Api.Auth;
using EventSphere.Api.Common;
using EventSphere.Api.Common.Options;
using EventSphere.Api.Data;
using EventSphere.Api.Middleware;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var env = builder.Environment;

// ---------- Options ----------
builder.Services.Configure<RefreshTokenOptions>(config.GetSection(RefreshTokenOptions.SectionName));
builder.Services.Configure<FrontendOptions>(config.GetSection(FrontendOptions.SectionName));
builder.Services.Configure<BrevoOptions>(config.GetSection(BrevoOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(config.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<StripeOptions>(config.GetSection(StripeOptions.SectionName));
var frontend = config.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>() ?? new FrontendOptions();

// ---------- Database ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// ---------- Identity (Core; JWT-first, no cookie login) ----------
builder.Services.AddIdentityCore<AppUser>(options =>
{
    // Strong password policy (authoritative). Documented in SECURITY.md.
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 4;

    options.User.RequireUniqueEmail = true;

    // Brute-force protection. Per-account lockout; values documented in SECURITY.md.
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    // Email verification is required before login.
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// ---------- JWT signing key resolution ----------
// Never hardcode a production secret. In Development an ephemeral key is generated
// if none is supplied so the app runs without setup; every other environment must
// provide Jwt:Key (>= 32 bytes) via env vars / user secrets / secret manager.
var jwtIssuer = config["Jwt:Issuer"] ?? "EventSphere";
var jwtAudience = config["Jwt:Audience"] ?? "EventSphere";
var accessTokenMinutes = config.GetValue("Jwt:AccessTokenMinutes", 15);
var jwtKey = config["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (env.IsDevelopment() || env.IsEnvironment("Testing"))
    {
        jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        Console.WriteLine("[WARN] Jwt:Key not configured; generated an ephemeral development key. Tokens will not survive restart.");
    }
    else
    {
        throw new InvalidOperationException("Jwt:Key must be configured outside Development.");
    }
}
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HMAC-SHA256.");

builder.Services.Configure<JwtOptions>(o =>
{
    o.Issuer = jwtIssuer;
    o.Audience = jwtAudience;
    o.Key = jwtKey;
    o.AccessTokenMinutes = accessTokenMinutes;
});

// ---------- Authentication: JWT (default) + external OAuth ----------
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // keep raw claim types ("sub", "role")
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

authBuilder.AddExternalOAuth(config);

builder.Services.AddAuthorization();

// ---------- Application services ----------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEngagementService, EngagementService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// ---------- Email service ----------
if (env.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IEmailService, NoOpEmailService>();
}
else
{
    builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();
}

// ---------- Error handling ----------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- Rate limiting ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General limit for all traffic (partitioned by client IP).
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var rl = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rl.GeneralPermitLimit,
                Window = TimeSpan.FromSeconds(rl.GeneralWindowSeconds),
                QueueLimit = 0
            });
    });

    // Stricter limit for sensitive auth endpoints.
    options.AddPolicy("auth", httpContext =>
    {
        var rl = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rl.AuthPermitLimit,
                Window = TimeSpan.FromSeconds(rl.AuthWindowSeconds),
                QueueLimit = 0
            });
    });

    // Dedicated limit for email-related endpoints (forgot-password, resend-verification).
    options.AddPolicy("email", httpContext =>
    {
        var rl = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rl.EmailPermitLimit,
                Window = TimeSpan.FromSeconds(rl.EmailWindowSeconds),
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, token) =>
    {
        var rl = context.HttpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra)
            ? (int)Math.Ceiling(ra.TotalSeconds)
            : rl.AuthWindowSeconds;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
        context.HttpContext.Response.ContentType = "application/json";

        var body = ApiResponse.Fail($"Too many requests. Please try again in {retryAfter} seconds.");
        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions(JsonSerializerDefaults.Web)), token);
    };

    static string ClientKey(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
});

// ---------- Controllers + uniform validation errors ----------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => JsonNamingPolicy.CamelCase.ConvertName(kv.Key),
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(ApiResponse.Fail("Validation failed.", errors));
        };
    });

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventSphere API",
        Version = "v1",
        Description = "EventSphere authentication & authorization API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- CORS (explicit origins from configuration; never AllowAnyOrigin) ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (frontend.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(frontend.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // required for the HttpOnly refresh cookie
        }
    });
});

var app = builder.Build();

// ---------- Middleware pipeline ----------
app.UseExceptionHandler();

if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ---------- Startup: migrate + seed (skipped under integration tests) ----------
if (!env.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(scope.ServiceProvider, config, logger);
    }
    catch (Exception ex)
    {
        // A missing/unreachable database must not crash startup silently; log clearly.
        logger.LogError(ex, "Database migrate/seed failed at startup. Verify the connection string and that SQL Server is reachable.");
    }
}

app.Run();

// Exposed for WebApplicationFactory in the test project.
public partial class Program { }
