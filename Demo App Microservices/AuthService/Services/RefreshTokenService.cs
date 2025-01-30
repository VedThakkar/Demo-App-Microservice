using AuthService.DTO;
using AuthService.GenerateToken;
using AuthService.Models;
using AuthService.Repositories.IRepositories;
using AuthService.Services.IServices;
using DemoApp1.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IGenericRepository<Refreshtoken> _refreshTokenRepository;
    private readonly TokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository1;

    public RefreshTokenService(IGenericRepository<Refreshtoken> refreshTokenRepository, TokenGenerator tokenGenerator, IUserRepository userRepository1)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenGenerator = tokenGenerator;
        _userRepository1 = userRepository1;
    }

    public async Task<string> RefreshToken(RefreshTokenDTO refreshtoken)
    {
        var refreshToken = refreshtoken.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenEntity = (await _refreshTokenRepository.GetAllAsync())
            .FirstOrDefault(rt => rt.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.ExpiresAt <= DateTime.UtcNow || tokenEntity.IsRevoked)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }
        var userId = tokenEntity.UserId;
        var user1 = await _refreshTokenRepository.Query()
            .Include(u => u.User) // Include navigation if necessary
            .Select(rt => rt.User)
            .SingleOrDefaultAsync(u => u.Id == userId);
        
        var user = await _userRepository1.GetUserWithRolesAsync(user1.Email);
        //Console.WriteLine("User: " + user1.Role.Name);
        var roleNames = user1.Role.Name;
        //Console.WriteLine("User: " + user1.Role.Name);
        
        var newAccessToken = _tokenGenerator.GenerateToken(user1.Id, user1.Email, roleNames);

        return newAccessToken;
    }
    
    public async Task RevokeToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenEntity = await _refreshTokenRepository
            .Query()
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null)
        {
            throw new ArgumentException("Invalid refresh token.");
        }

        tokenEntity.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(tokenEntity);
    }
}