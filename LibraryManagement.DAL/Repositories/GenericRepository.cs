using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly LibraryDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(LibraryDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}
