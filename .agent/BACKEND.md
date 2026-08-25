# BACKEND.md — ASP.NET Core Web API

## Overview

Pure **ASP.NET Core 8 Web API**. No MVC controllers, no Razor Views.

## Project Structure

```
EventSphere.Api/
├── Program.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   ├── RegistrationsController.cs
│   ├── AttendancesController.cs
│   ├── CertificatesController.cs
│   ├── FeedbackController.cs
│   ├── MediaController.cs
│   ├── UsersController.cs
│   ├── NotificationsController.cs
│   ├── DashboardController.cs
│   └── VenuesController.cs
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/Entities/
├── DTOs/
├── Hubs/NotificationHub.cs
└── appsettings.json
```

## Program.cs

```csharp
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

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// API Controllers
builder.Services.AddControllers();
```

## Team Ownership

| Area | Owner |
|---|---|
| Core architecture, auth, middleware | Abdullah |
| Database schema, EF Core, data services | Jibran |
| API controllers | Abdullah + Jibran |
