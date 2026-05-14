using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;

    public StudentService(IRepository<Student> repository)
    {
        _repository = repository;
    }

    public async Task<List<StudentResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.ToResponseDtoList();
    }

    public async Task<PagedResult<StudentResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s)
                           || e.FullName.ToLower().Contains(s)
                           || e.Email.ToLower().Contains(s)
                           || e.Phone.ToLower().Contains(s)
                           || e.Address.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            q = query.SortBy.ToLower() switch
            {
                "code" => query.SortDesc ? q.OrderByDescending(e => e.Code) : q.OrderBy(e => e.Code),
                "fullname" => query.SortDesc ? q.OrderByDescending(e => e.FullName) : q.OrderBy(e => e.FullName),
                "email" => query.SortDesc ? q.OrderByDescending(e => e.Email) : q.OrderBy(e => e.Email),
                _ => q.OrderBy(e => e.Id)
            };
        }
        else
        {
            q = q.OrderBy(e => e.Id);
        }

        var totalItems = await q.CountAsync();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var entities = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StudentResponse>
        {
            Items = entities.ToResponseDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<StudentResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToResponseDto();
    }

    public async Task<StudentResponse> AddAsync(StudentRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToResponseDto();
    }

    public async Task<StudentResponse?> UpdateAsync(int id, StudentRequest request)
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
