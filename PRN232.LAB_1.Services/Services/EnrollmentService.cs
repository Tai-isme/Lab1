using Microsoft.EntityFrameworkCore;
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

        // Expand — apply Include before ordering
        if (!string.IsNullOrWhiteSpace(query.Expand))
        {
            var expand = query.Expand.Split(',', StringSplitOptions.TrimEntries);
            if (expand.Contains("student"))
                q = q.Include(e => e.Student);
            if (expand.Contains("course"))
                q = q.Include(e => e.Course);
        }

        // Sort
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            q = query.SortBy.ToLower() switch
            {
                "enrollmentdate" => query.SortDesc ? q.OrderByDescending(e => e.EnrollmentDate) : q.OrderBy(e => e.EnrollmentDate),
                "status" => query.SortDesc ? q.OrderByDescending(e => e.Status) : q.OrderBy(e => e.Status),
                "grade" => query.SortDesc ? q.OrderByDescending(e => e.Grade!) : q.OrderBy(e => e.Grade!),
                _ => q.OrderBy(e => e.Id)
            };
        }
        else
        {
            q = q.OrderBy(e => e.Id);
        }

        // Count
        var totalItems = await q.CountAsync();

        // Page
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var entities = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map with expand
        var expandArr = !string.IsNullOrWhiteSpace(query.Expand)
            ? query.Expand.Split(',', StringSplitOptions.TrimEntries)
            : [];

        return new PagedResult<EnrollmentResponse>
        {
            Items = entities.ToResponseDtoList(expandArr),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
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
