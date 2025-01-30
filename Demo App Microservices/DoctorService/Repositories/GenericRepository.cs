using DoctorService.Data;
using DoctorService.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DoctorService.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DoctorDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DoctorDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var existingEntity = await _dbSet.FindAsync(entity.GetType().GetProperty("Id").GetValue(entity));
        if (existingEntity != null)
        {
            // Detach the existing entity from the DbContext to avoid tracking conflicts
            _context.Entry(existingEntity).State = EntityState.Detached;
        }
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
    
    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
