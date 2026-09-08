using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantFlow.Api.Hubs;
using RestaurantFlow.Api.Services;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Application.Services;
using RestaurantFlow.Infrastructure.Data;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Database - Detecta automáticamente SQLite o SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString?.Contains("Data Source=") == true)
    {
        // SQLite detectado
        options.UseSqlite(connectionString);
    }
    else
    {
        // SQL Server por defecto
        options.UseSqlServer(connectionString);
    }
});

// Registrar interfaz IAppDbContext
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHereMustBeLongEnough123456";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RestaurantFlowApi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RestaurantFlowClient",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// CORS - Configurado para SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173", "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Necesario para SignalR
    });
});

// SignalR
builder.Services.AddSignalR();

// SignalR configuration (can be disabled in appsettings.json)
var signalREnabled = builder.Configuration.GetValue<bool>("SignalR:Enabled", false);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RestaurantFlow API",
        Version = "v1.0",
        Description = "REST API for comprehensive restaurant management: authentication, real-time order management, reservations, notifications, and reporting.",
        Contact = new OpenApiContact
        {
            Name = "RestaurantFlow",
            Email = "support@restaurantflow.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Configurar Bearer Authentication en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the field below.\n\n" +
                      "Format: Bearer {token}\n\n" +
                      "Example: Bearer eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Crear la base de datos si no existe
        await context.Database.EnsureCreatedAsync();
        
        // Insertar datos demo
        await DataSeeder.SeedAsync(context);
        Log.Information("Base de datos inicializada correctamente con datos demo");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al inicializar la base de datos");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestaurantFlow API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "RestaurantFlow API";
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
});

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub
app.MapHub<RestaurantHub>("/hubs/restaurant");

if (signalREnabled)
{
    Log.Information("RestaurantFlow API started with SignalR enabled");
}
else
{
    Log.Information("RestaurantFlow API started successfully");
}

app.Run();
