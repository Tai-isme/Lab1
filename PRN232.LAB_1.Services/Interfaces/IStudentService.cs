using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface IStudentService
{
    Task<List<StudentResponse>> GetAllAsync();
    Task<StudentResponse?> GetByIdAsync(int id);
    Task<StudentResponse> AddAsync(StudentRequest request);
    Task<StudentResponse?> UpdateAsync(int id, StudentRequest request);
    Task<bool> DeleteAsync(int id);
}
