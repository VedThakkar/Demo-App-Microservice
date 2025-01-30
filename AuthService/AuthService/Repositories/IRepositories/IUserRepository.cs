using AuthService.Models;
using AuthService.Repositories.IRepositories;

namespace DemoApp1.Repositories.IRepositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetUserWithRolesAsync(string email);
}