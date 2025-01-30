using AuthService.DTO;
using AuthService.Services.IServices;
using FluentValidation;
using Serilog;

namespace AuthService.APIs;

public static class RefreshTokenEndpoints
{
    public static IEndpointRouteBuilder MapRefreshTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/refresh", async (RefreshTokenDTO request, IRefreshTokenService refreshTokenService) =>
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return Results.BadRequest("Refresh token is required.");
                }
                var newAccessToken = await refreshTokenService.RefreshToken(new RefreshTokenDTO
                {
                    RefreshToken = request.RefreshToken
                });

                return Results.Ok(new { access_token = newAccessToken });
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Unauthorized Access.");
                return Results.Unauthorized();
            }
            catch (ValidationException ex)
            {
                Log.Warning("Validation failed for Getting new access token.");
                return Results.BadRequest(new { Message = "Invalid refresh token data", Errors = ex.Errors });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
        
        endpoints.MapPost("/revoke", async (RefreshTokenDTO request, IRefreshTokenService refreshTokenService) =>
        {
            try
            {
                await refreshTokenService.RevokeToken(request.RefreshToken);
                return Results.Ok("Refresh token revoked.");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
        
        return endpoints;
    }
}