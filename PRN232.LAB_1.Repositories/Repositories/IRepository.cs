using System.Linq;

namespace PRN232.LAB_1.Repositories.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(int id, string[]? includes);
    T Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> GetQueryable();
}
