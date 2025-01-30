using System.Text.Json.Serialization;
using AuthService.APIs;
using AuthService.Consumers;
using AuthService.Data;
using AuthService.GenerateToken;
using AuthService.Messages;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Repositories.IRepositories;
using AuthService.Services;
using AuthService.Services.IServices;
using AuthService.Validators;
using AuthService.Validators.IValidators;
using DemoApp1.Repositories.IRepositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
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
builder.Services.AddSingleton<TokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddTransient<IValidatorService<User>,UserValidator>();
builder.Services.AddTransient<IValidatorService<Role>,RoleValidator>();
builder.Services.AddTransient<IValidatorService<Refreshtoken>,RefreshtokenValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 27)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));

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

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PatientDeletedConsumer>();
    config.AddConsumer<DoctorDeletedConsumer>();
    config.AddConsumer<PatientLoggedInConsumer>();
    config.AddConsumer<DoctorLoggedInConsumer>();
    
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
            x.SetEntityName("Registered");
        });
        
        cfg.Message<PatientDeleted>(x =>
        {
            x.SetEntityName("PatientDeleted");
        });
        
        cfg.Message<DoctorDeleted>(x =>
        {
            x.SetEntityName("DoctorDeleted");
        });
        
        cfg.Message<PatientLoggedIn>(x =>
        {
            x.SetEntityName("PatientLoggedIn");
        });
        
        cfg.Message<DoctorLoggedIn>(x =>
        {
            x.SetEntityName("DoctorLoggedIn");
        });
        
        
        cfg.ReceiveEndpoint("patient-deleted-queue", e =>
        {
            e.ConfigureConsumer<PatientDeletedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("doctor-deleted-queue", e =>
        {
            e.ConfigureConsumer<DoctorDeletedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("patient-logged-in-queue", e =>
        {
            e.ConfigureConsumer<PatientLoggedInConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("doctor-logged-in-queue", e =>
        {
            e.ConfigureConsumer<DoctorLoggedInConsumer>(context);
        });
        
        cfg.ConfigureEndpoints(context);
        
        cfg.Publish<UserLoggedIn>(s =>
        {
            s.ExchangeType = ExchangeType.Direct;
        });
        
        cfg.Publish<UserRegistered>(s =>
        {
            s.ExchangeType = ExchangeType.Direct;
        });
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAuthEndpoints();
app.MapRefreshTokenEndpoints();

app.Run();