using AuthService.DTO;

namespace AuthService.Services.IServices;

public interface IRefreshTokenService
{
    Task<string> RefreshToken(RefreshTokenDTO refreshtoken);

    Task RevokeToken(string refreshToken);
}