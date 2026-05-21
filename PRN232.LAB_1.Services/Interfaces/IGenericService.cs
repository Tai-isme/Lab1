using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface IGenericService<TResponse, TRequest>
    where TResponse : class
    where TRequest : class
{
    Task<List<TResponse>> GetAllAsync();
    Task<PagedResult<TResponse>> GetAllAsync(PagedQuery query);
    Task<TResponse?> GetByIdAsync(int id);
    Task<TResponse?> GetByIdAsync(int id, string? expand);
    Task<TResponse> AddAsync(TRequest request);
    Task<TResponse?> UpdateAsync(int id, TRequest request);
    Task<bool> DeleteAsync(int id);
}
