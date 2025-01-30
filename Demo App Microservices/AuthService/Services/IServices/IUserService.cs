using AuthService.Models;

namespace AuthService.Services.IServices;

public interface IUserService
{
    Task<User> DeleteUserById(int UserId);
}