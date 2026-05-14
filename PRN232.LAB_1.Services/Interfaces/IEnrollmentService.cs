using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Interfaces;

public interface IEnrollmentService
{
    Task<List<EnrollmentResponse>> GetAllAsync();
    Task<EnrollmentResponse?> GetByIdAsync(int id);
    Task<EnrollmentResponse> AddAsync(EnrollmentRequest request);
    Task<EnrollmentResponse?> UpdateAsync(int id, EnrollmentRequest request);
    Task<bool> DeleteAsync(int id);
}
