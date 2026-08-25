# BACKEND.md — ASP.NET Core Backend

## Project Structure

```
EventSphere.Web/
├── Program.cs                          # App entry point, DI, middleware
├── Controllers/                        # MVC Controllers
│   ├── AccountController.cs            # Login, Register, Profile, Logout
│   ├── EventsController.cs             # CRUD + Registration
│   ├── HomeController.cs               # Landing page
│   ├── TicketsController.cs            # Ticket management
│   ├── DatabaseController.cs           # Seed database
│   └── Api/                            # Web API Controllers
│       ├── AuthApiController.cs        # JWT login/register
│       ├── EventsApiController.cs      # Event CRUD API
│       ├── NotificationsApiController.cs
│       ├── ReviewsApiController.cs
│       └── Dtos/ApiDtos.cs             # All DTOs
├── Services/
│   ├── Interfaces/                     # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IEventService.cs
│   │   ├── INotificationService.cs
│   │   ├── IRegistrationService.cs
│   │   ├── IReviewService.cs
│   │   ├── ITicketService.cs
│   │   └── IVenueService.cs
│   └── Implementations/               # Service implementations
│       ├── AuthService.cs
│       ├── EventService.cs
│       ├── NotificationService.cs
│       ├── RegistrationService.cs
│       ├── ReviewService.cs
│       ├── TicketService.cs
│       └── VenueService.cs
├── Data/
│   ├── ApplicationDbContext.cs         # EF Core DbContext
│   └── SeedData.cs                     # Initial data
├── Hubs/
│   └── NotificationHub.cs             # SignalR hub
├── Models/
│   └── Entities/                       # Domain models
│       ├── AppUser.cs
│       ├── Event.cs
│       ├── EventCategory.cs
│       ├── EventRegistration.cs
│       ├── Ticket.cs
│       ├── Payment.cs
│       ├── Venue.cs
│       ├── Review.cs
│       └── Notification.cs
├── ViewModels/                         # View models for Razor
├── wwwroot/                            # Static assets
├── appsettings.json
└── appsettings.Development.json
```

## Program.cs Configuration

Key registrations in `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication (Cookie for MVC, JWT for API)
builder.Services.AddAuthentication()
    .AddCookie(...)
    .AddJwtBearer(...);

// Services (DI)
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// SignalR
builder.Services.AddSignalR();

// MVC
builder.Services.AddControllersWithViews();
```

## Layers

### MVC Layer
- Controllers inherit from `Controller`
- Return `View()` or `RedirectToAction()`
- Use `[ValidateAntiForgeryToken]` on POST
- Use `[Authorize]` for protected pages

### API Layer
- Controllers inherit from `ControllerBase`
- Use `[ApiController]` + `[Route("api/[controller]")]`
- Return `Ok()`, `Created()`, `NotFound()`, `BadRequest()`
- Use `[Authorize]` for protected endpoints

### Service Layer
- Registered as Scoped in DI
- Handle business logic
- Use async/await
- Return domain objects or DTOs

### Data Layer
- `ApplicationDbContext` inherits `IdentityDbContext<AppUser>`
- Entities mapped in `OnModelCreating`
- Seed data in `SeedData.cs`

## Configuration

- `appsettings.json` — production config
- `appsettings.Development.json` — development overrides
- Connection string: `ConnectionStrings:DefaultConnection`
- JWT: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationInMinutes`

> Never commit real secrets. Use placeholders.
