using AuthService.Models;
using AuthService.Repositories.IRepositories;
using AuthService.Services.IServices;

namespace AuthService.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> DeleteUserById(int UserId)
    {
        var user = await _userRepository.GetByIdAsync(UserId);
        if (user == null)
        {
            return null;
        }

        await _userRepository.DeleteAsync(UserId);
        return user;
    }
}