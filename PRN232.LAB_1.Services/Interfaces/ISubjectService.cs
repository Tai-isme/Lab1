using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface ISubjectService
{
    Task<List<SubjectResponse>> GetAllAsync();
    Task<SubjectResponse?> GetByIdAsync(int id);
    Task<SubjectResponse> AddAsync(SubjectRequest request);
    Task<SubjectResponse?> UpdateAsync(int id, SubjectRequest request);
    Task<bool> DeleteAsync(int id);
}
