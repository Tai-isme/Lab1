using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface ICourseService
{
    Task<List<CourseResponse>> GetAllAsync();
    Task<PagedResult<CourseResponse>> GetAllAsync(PagedQuery query);
    Task<CourseResponse?> GetByIdAsync(int id);
    Task<CourseResponse> AddAsync(CourseRequest request);
    Task<CourseResponse?> UpdateAsync(int id, CourseRequest request);
    Task<bool> DeleteAsync(int id);
}
