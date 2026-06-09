using System.Linq.Expressions;
using fifa_backend.Models;

namespace fifa_backend.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
    Task SaveChangesAsync();
}
