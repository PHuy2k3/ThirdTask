using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using CategoryApi.Data; 

namespace CategoryApi.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;  
    private readonly DbSet<T> _set;

    public EfRepository(AppDbContext db) 
    {
        _db = db;
        _set = db.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();
    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await _set.FindAsync(new object?[] { id }, ct);
    public Task AddAsync(T e, CancellationToken ct = default) => _set.AddAsync(e, ct).AsTask();
    public void Update(T e) => _set.Update(e);
    public void Remove(T e) => _set.Remove(e);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    public Task<bool> AnyAsync(Expression<Func<T, bool>> p, CancellationToken ct = default) => _set.AnyAsync(p, ct);
}
