using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Helpers;
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
        return entities.Select(e => e.ToBusinessModel()).ToResponseDtoList();
    }

    public async Task<PagedResult<SubjectResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _repository.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s)
                           || e.Name.ToLower().Contains(s)
                           || e.Description.ToLower().Contains(s));
        }

        // Expand stub — Subject has no forward navigation properties to expand

        // Sort — per D-01
        var sortFields = new Dictionary<string, (Func<IQueryable<Subject>, IOrderedQueryable<Subject>> asc, Func<IQueryable<Subject>, IOrderedQueryable<Subject>> desc)>
        {
            { "id", (q => q.OrderBy(e => e.Id), q => q.OrderByDescending(e => e.Id)) },
            { "code", (q => q.OrderBy(e => e.Code), q => q.OrderByDescending(e => e.Code)) },
            { "name", (q => q.OrderBy(e => e.Name), q => q.OrderByDescending(e => e.Name)) },
            { "credits", (q => q.OrderBy(e => e.Credits), q => q.OrderByDescending(e => e.Credits)) },
        };
        var ordered = q.ApplyMultiFieldSort(query.Sort, sortFields);

        // Count
        var totalItems = await ordered.CountAsync();

        // Page
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var entities = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map and apply field selection — per D-02
        var items = entities
            .Select(e => e.ToBusinessModel())
            .ToResponseDtoList()
            .ApplyFieldSelection(query.Fields)
            .ToList();

        return new PagedResult<SubjectResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<SubjectResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : null;
        var entity = await _repository.GetByIdAsync(id, includes);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<SubjectResponse> AddAsync(SubjectRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToBusinessModel().ToResponseDto();
    }

    public async Task<SubjectResponse?> UpdateAsync(int id, SubjectRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        request.UpdateEntity(entity);
        await _repository.UpdateAsync(entity);
        return entity.ToBusinessModel().ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}
