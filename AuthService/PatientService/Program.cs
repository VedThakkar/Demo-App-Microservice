
using System.Security.Claims;
using System.Text.Json.Serialization;
using AuthService.Messages;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientService.APIs;
using PatientService.AuthProvider;
using PatientService.AuthProvider.IAuthProvider;
using PatientService.Consumers;
using PatientService.Data;
using PatientService.Models;
using PatientService.Repositories;
using PatientService.Repositories.IRepositories;
using PatientService.Services.IServices;
using PatientService.Validators;
using PatientService.Validators.IValidators;
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
builder.Services.AddScoped<IPatientService, PatientService.Services.PatientService>();
builder.Services.AddTransient<IValidatorService<Patient>, PatientValidator>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsProvider, ClaimsProvider>();

builder.Services.AddDbContext<PatientDbContext>(options =>
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
            x.SetEntityName("Registered");
        });
        
        cfg.Message<PatientDeleted>(x =>
        {
            x.SetEntityName("PatientDeleted");
        });
        
        cfg.Message<PatientLoggedIn>(x =>
        {
            x.SetEntityName("PatientLoggedIn");
        });
        
        // Declare a direct exchange for the UserLoggedIn message
        cfg.ReceiveEndpoint("user-logged-in-queue-patient", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer<UserLoggedInConsumer>(context);

            e.Bind("LoggedIn", s =>
            {
                s.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
                s.RoutingKey = "patient";  // Patient routing key
                s.Durable = true;
                s.AutoDelete = false;
            });
        });
        
        cfg.ReceiveEndpoint("user-register-queue-patient", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer<UserRegisteredConsumer>(context);

            e.Bind("Registered", s =>
            {
                s.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
                s.RoutingKey = "patient";  // Patient routing key
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
    options.AddPolicy("AdminPatient", policy => policy.RequireRole("admin", "patient"));
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



var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPatientEndpoints();


app.Run();