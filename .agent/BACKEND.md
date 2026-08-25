# BACKEND.md — ASP.NET Core Web API

## Overview

The backend is a pure **ASP.NET Core 8 Web API**. No MVC controllers, no Razor Views. All UI is served by the React frontend.

## Project Structure

```
EventSphere.Api/
├── Program.cs                          # Entry point, DI, middleware
├── Controllers/                        # API Controllers only
│   ├── AuthController.cs               # Login, register, token refresh
│   ├── EventsController.cs             # Event CRUD
│   ├── RegistrationsController.cs      # Event registration
│   ├── AttendancesController.cs        # QR check-in
│   ├── CertificatesController.cs       # Certificate management
│   ├── FeedbackController.cs           # Reviews/feedback
│   ├── MediaController.cs              # Gallery upload
│   ├── UsersController.cs              # User management (admin)
│   ├── NotificationsController.cs      # User notifications
│   ├── DashboardController.cs          # Admin/organizer analytics
│   └── VenuesController.cs             # Venue management
├── Services/
│   ├── Interfaces/                     # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IEventService.cs
│   │   ├── IRegistrationService.cs
│   │   ├── IAttendanceService.cs
│   │   ├── ICertificateService.cs
│   │   ├── IFeedbackService.cs
│   │   ├── IMediaService.cs
│   │   ├── IUserService.cs
│   │   ├── INotificationService.cs
│   │   ├── IDashboardService.cs
│   │   └── IVenueService.cs
│   └── Implementations/               # Service implementations
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/
│   └── Entities/                       # Domain models
├── DTOs/                               # Request/Response DTOs
├── Hubs/
│   └── NotificationHub.cs
├── Middleware/                          # Custom middleware
├── appsettings.json
└── appsettings.Development.json
```

## Program.cs Configuration

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// CORS for React
builder.Services.AddCors(options =>
{
    options.AddPolicy("React", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Services (DI)
builder.Services.AddScoped<IEventService, EventService>();
// ... other services

// SignalR
builder.Services.AddSignalR();

// API Controllers
builder.Services.AddControllers();
```

## Middleware Pipeline

```csharp
app.UseHttpsRedirection();
app.UseCors("React");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
```

## Layers

### Controller Layer
- `[ApiController]` + `[Route("api/[controller]")]`
- Thin controllers — delegate to services
- `[Authorize]` on protected endpoints
- Return `Ok()`, `Created()`, `NotFound()`, `BadRequest()`

### Service Layer
- Registered as Scoped in DI
- Handle business logic
- Async/await throughout
- Return DTOs or domain objects

### Data Layer
- `ApplicationDbContext` (EF Core)
- Entities mapped in `OnModelCreating`
- Migrations for schema changes

## CORS

- Development: `http://localhost:5173` (Vite default)
- Production: configured via environment variable
- Allow credentials for JWT cookies if used

## Configuration

- `appsettings.json` — production config
- `appsettings.Development.json` — dev overrides
- Environment variables for secrets in production

> Never commit real secrets. Use placeholders.
