using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface ISemesterService
{
    Task<List<SemesterResponse>> GetAllAsync();
    Task<PagedResult<SemesterResponse>> GetAllAsync(PagedQuery query);
    Task<SemesterResponse?> GetByIdAsync(int id);
    Task<SemesterResponse> AddAsync(SemesterRequest request);
    Task<SemesterResponse?> UpdateAsync(int id, SemesterRequest request);
    Task<bool> DeleteAsync(int id);
}
