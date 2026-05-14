using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class CourseService : ICourseService
{
    private readonly IRepository<Course> _repository;

    public CourseService(IRepository<Course> repository)
    {
        _repository = repository;
    }

    public async Task<List<CourseResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.ToResponseDtoList();
    }

    public async Task<PagedResult<CourseResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _repository.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s)
                           || e.Instructor.ToLower().Contains(s)
                           || e.Room.ToLower().Contains(s)
                           || e.Schedule.ToLower().Contains(s));
        }

        // Expand — apply Include before ordering
        if (!string.IsNullOrWhiteSpace(query.Expand))
        {
            var expand = query.Expand.Split(',', StringSplitOptions.TrimEntries);
            if (expand.Contains("subject"))
                q = q.Include(e => e.Subject);
            if (expand.Contains("semester"))
                q = q.Include(e => e.Semester);
        }

        // Sort
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            q = query.SortBy.ToLower() switch
            {
                "code" => query.SortDesc ? q.OrderByDescending(e => e.Code) : q.OrderBy(e => e.Code),
                "instructor" => query.SortDesc ? q.OrderByDescending(e => e.Instructor) : q.OrderBy(e => e.Instructor),
                "room" => query.SortDesc ? q.OrderByDescending(e => e.Room) : q.OrderBy(e => e.Room),
                "maxstudents" => query.SortDesc ? q.OrderByDescending(e => e.MaxStudents) : q.OrderBy(e => e.MaxStudents),
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

        return new PagedResult<CourseResponse>
        {
            Items = entities.ToResponseDtoList(expandArr),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CourseResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToResponseDto();
    }

    public async Task<CourseResponse> AddAsync(CourseRequest request)
    {
        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity);
        return created.ToResponseDto();
    }

    public async Task<CourseResponse?> UpdateAsync(int id, CourseRequest request)
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
