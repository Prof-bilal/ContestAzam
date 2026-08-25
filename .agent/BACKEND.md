# BACKEND.md — ASP.NET Core Backend

## Overview

The backend is a single ASP.NET Core 8 project with both MVC Controllers (for Razor Views) and Web API Controllers (for JSON endpoints).

## Project Structure

```
EventSphere.Web/
├── Program.cs
├── Controllers/
│   ├── HomeController.cs           # Landing page
│   ├── AccountController.cs        # Login, Register, Profile
│   ├── EventsController.cs         # Event listing, detail, create
│   ├── TicketsController.cs        # Ticket management
│   ├── DashboardController.cs      # User/organizer/admin dashboards
│   ├── GalleryController.cs        # Media gallery
│   ├── FeedbackController.cs       # Reviews
│   ├── AdminController.cs          # Admin panel
│   └── Api/                        # Web API controllers
│       ├── AuthApiController.cs
│       ├── EventsApiController.cs
│       ├── RegistrationsApiController.cs
│       ├── NotificationsApiController.cs
│       └── Dtos/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/Entities/
├── ViewModels/
├── Views/
├── Hubs/
├── wwwroot/
└── appsettings.json
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

// Authentication (Cookie for MVC, JWT for API)
builder.Services.AddAuthentication()
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
    })
    .AddJwtBearer(options => { ... });

// Services (DI)
builder.Services.AddScoped<IEventService, EventService>();
// ... other services

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

### Service Layer
- Registered as Scoped in DI
- Handle business logic
- Use async/await

### Data Layer
- `ApplicationDbContext` (EF Core)
- Entities mapped in `OnModelCreating`
- Migrations for schema changes

## Team Ownership

| Area | Owner |
|---|---|
| Core architecture, auth, middleware | Abdullah |
| Database schema, EF Core, data services | Jibran |
| Shared layout, auth UI | Ramsha |
| Feature UI, dashboards | Marukh |
