using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Helpers;
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
        return entities.Select(e => e.ToBusinessModel()).ToResponseDtoList();
    }

    public async Task<PagedResult<EnrollmentResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _repository.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Status.ToLower().Contains(s)
                           || (e.Grade != null && e.Grade.ToLower().Contains(s)));
        }

        // Expand — per D-03 (already existed, keep same pattern)
        if (!string.IsNullOrWhiteSpace(query.Expand))
        {
            var expand = query.Expand.Split(',', StringSplitOptions.TrimEntries);
            if (expand.Contains("student"))
                q = q.Include(e => e.Student);
            if (expand.Contains("course"))
                q = q.Include(e => e.Course);
        }

        // Sort — per D-01 (replaces old switch-based sort)
        var sortFields = new Dictionary<string, (Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>> asc, Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>> desc)>
        {
            { "id", (q => q.OrderBy(e => e.Id), q => q.OrderByDescending(e => e.Id)) },
            { "enrollmentdate", (q => q.OrderBy(e => e.EnrollmentDate), q => q.OrderByDescending(e => e.EnrollmentDate)) },
            { "status", (q => q.OrderBy(e => e.Status), q => q.OrderByDescending(e => e.Status)) },
            { "grade", (q => q.OrderBy(e => e.Grade!), q => q.OrderByDescending(e => e.Grade!)) },
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

        // Map with expand and apply field selection
        var expandArr = !string.IsNullOrWhiteSpace(query.Expand)
            ? query.Expand.Split(',', StringSplitOptions.TrimEntries)
            : [];

        var items = entities
            .Select(e => e.ToBusinessModel())
            .ToResponseDtoList(expandArr)
            .ApplyFieldSelection(query.Fields)
            .ToList();

        return new PagedResult<EnrollmentResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<EnrollmentResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<EnrollmentResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : null;
        var entity = await _repository.GetByIdAsync(id, includes);
        if (entity == null) return null;

        var expandArr = includes ?? [];
        return entity.ToBusinessModel().ToResponseDto(expandArr);
    }

    public async Task<EnrollmentResponse> AddAsync(EnrollmentRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToBusinessModel().ToResponseDto();
    }

    public async Task<EnrollmentResponse?> UpdateAsync(int id, EnrollmentRequest request)
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
