using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IRepository<Enrollment> _repository;

    public EnrollmentService(IRepository<Enrollment> repository)
    {
        _repository = repository;
    }

    public async Task<List<EnrollmentResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.ToResponseDtoList();
    }

    public async Task<EnrollmentResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToResponseDto();
    }

    public async Task<EnrollmentResponse> AddAsync(EnrollmentRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToResponseDto();
    }

    public async Task<EnrollmentResponse?> UpdateAsync(int id, EnrollmentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        request.UpdateEntity(entity);
        await _repository.UpdateAsync(entity);
        return entity.ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}
