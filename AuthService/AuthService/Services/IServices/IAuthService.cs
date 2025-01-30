using AuthService.DTO;
using AuthService.Models;

namespace AuthService.Services.IServices;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginUserAsync(LoginRequestDTO loginRequestDto);
    Task<User> RegisterUserAsync(RegisterRequestDTO request);
}