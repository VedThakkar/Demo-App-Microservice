using AuthService.Data;
using AuthService.Models;
using DemoApp1.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserWithRolesAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}