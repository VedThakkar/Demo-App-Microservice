using AuthService.DTO;
using AuthService.Services.IServices;
using FluentValidation;
using Serilog;

namespace AuthService.APIs;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/login", async (LoginRequestDTO request, IAuthService authService) =>
        {
            try
            {
                Log.Information("User {Email} is attempting to log in.", request.Email);  // Log username with structured logging
                // Call the AuthService to log the user in
                var (accessToken, refreshToken) = await authService.LoginUserAsync(request);
                
                // Log successful login
                Log.Information("User {Email} successfully logged in.", request.Email);

                // Return a success response with tokens
                return Results.Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, "Unauthorized access attempt for user {Email}.", request.Email);  // Log unauthorized access
                // Return 401 if login fails
                return Results.Unauthorized();
            }
            catch (ValidationException ex)
            {
                Log.Warning("Validation failed for Registering User.");
                return Results.BadRequest(new { Message = "Invalid user data", Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while logging in user {Email}.", request.Email);  // Log any other errors
                // Return 500 for any other errors
                return Results.Problem(ex.Message);
            }
        });
        
        endpoints.MapPost("/register", async (RegisterRequestDTO request, IAuthService authService) =>
        {
            try
            {
                Log.Information("Registering a new user with email {Email}.", request.Email);  // Log registration attempt with email
                // Call the AuthService to register the user
                var user = await authService.RegisterUserAsync(request);
                
                // Log successful registration
                Log.Information("User {Email} successfully registered with ID {UserId}.", request.Email, user.Id);

                // Return a success response with the created user
                return Results.Created($"/users/{user.Id}", user);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Invalid operation during registration for email {Email}.", request.Email);  // Log specific error for invalid operation
                // Return 400 if registration fails (e.g., email already in use)
                return Results.BadRequest(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                Log.Warning("Validation failed for Registering User.");
                return Results.BadRequest(new { Message = "Invalid user data", Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while registering user {Email}.", request.Email);  // Log any other errors
                // Return 500 for any other errors
                return Results.Problem(ex.Message);
            }
        });
        
        
        
        return endpoints;
    }
    
}