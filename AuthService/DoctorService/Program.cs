using System.Text.Json.Serialization;
using AuthService.Messages;
using DoctorService.APIs;
using DoctorService.AuthProvider;
using DoctorService.AuthProvider.IAuthProvider;
using DoctorService.Consumers;
using DoctorService.Data;
using DoctorService.Models;
using DoctorService.Repositories;
using DoctorService.Repositories.IRepositories;
using DoctorService.Services.IServices;
using DoctorService.Validators;
using DoctorService.Validators.IValidators;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging, reading configuration from appsettings.json
builder.Host.UseSerilog((context, services, configuration) =>
{
    // Reads configuration settings for Serilog from appsettings.json
    // This enables setting options such as log levels, sinks, and output formats directly from the configuration file.
    configuration.ReadFrom.Configuration(context.Configuration);
});
// Register services and configure Newtonsoft.Json to handle circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use System.Text.Json to handle circular references.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
    });
builder.Services.AddEndpointsApiExplorer();  // For OpenAPI (Swagger) support (optional)
builder.Services.AddSwaggerGen();  // Add Swagger support (optional)
builder.Services.AddLogging();

builder.Services.AddScoped<IDoctorService, DoctorService.Services.DoctorService>();
builder.Services.AddTransient<IValidatorService<Doctor>, DoctorValidator>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsProvider, ClaimsProvider>();

builder.Services.AddDbContext<DoctorDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 27)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));


builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserLoggedInConsumer>();
    config.AddConsumer<UserRegisteredConsumer>();
    
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.Message<UserLoggedIn>(x =>
        {
            x.SetEntityName("LoggedIn");
        });
        cfg.Message<UserRegistered>(x =>
        {
            x.SetEntityName("LoggedIn");
        });
        
        cfg.Message<DoctorDeleted>(x =>
        {
            x.SetEntityName("DoctorDeleted");
        });
        
        cfg.Message<DoctorLoggedIn>(x =>
        {
            x.SetEntityName("DoctorLoggedIn");
        });
        
        // Declare a direct exchange for the UserLoggedIn message
        cfg.ReceiveEndpoint("user-logged-in-queue-doctor", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer<UserLoggedInConsumer>(context);

            e.Bind("LoggedIn", s =>
            {
                s.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
                s.RoutingKey = "doctor";  // Doctor routing key
                s.Durable = true;
                s.AutoDelete = false;
            });
        });
        cfg.ReceiveEndpoint("user-register-queue-doctor", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer<UserRegisteredConsumer>(context);

            e.Bind("Registered", s =>
            {
                s.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
                s.RoutingKey = "doctor";  // Patient routing key
                s.Durable = true;
                s.AutoDelete = false;
            });
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("AdminDoctor", policy => policy.RequireRole("admin", "doctor"));
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
    });


builder.WebHost.ConfigureKestrel(options =>
{
    // Change the port to something else, like 5058
    options.ListenLocalhost(5058);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDoctorEndpoints();

app.Run();