using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<Subject> _repository;

    public SubjectService(IRepository<Subject> repository)
    {
        _repository = repository;
    }

    public async Task<List<SubjectResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.ToResponseDtoList();
    }

    public async Task<SubjectResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToResponseDto();
    }

    public async Task<SubjectResponse> AddAsync(SubjectRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToResponseDto();
    }

    public async Task<SubjectResponse?> UpdateAsync(int id, SubjectRequest request)
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
