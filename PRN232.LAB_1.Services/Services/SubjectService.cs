using Microsoft.EntityFrameworkCore;
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

    public async Task<PagedResult<SubjectResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s)
                           || e.Name.ToLower().Contains(s)
                           || e.Description.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            q = query.SortBy.ToLower() switch
            {
                "code" => query.SortDesc ? q.OrderByDescending(e => e.Code) : q.OrderBy(e => e.Code),
                "name" => query.SortDesc ? q.OrderByDescending(e => e.Name) : q.OrderBy(e => e.Name),
                "credits" => query.SortDesc ? q.OrderByDescending(e => e.Credits) : q.OrderBy(e => e.Credits),
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

        return new PagedResult<SubjectResponse>
        {
            Items = entities.ToResponseDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
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
