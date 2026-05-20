using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Helpers;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CourseResponse>> GetAllAsync()
    {
        var entities = await _unitOfWork.Courses.GetAllAsync();
        return entities.Select(e => e.ToBusinessModel()).ToResponseDtoList();
    }

    public async Task<PagedResult<CourseResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _unitOfWork.Courses.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s)
                           || e.Instructor.ToLower().Contains(s)
                           || e.Room.ToLower().Contains(s)
                           || e.Schedule.ToLower().Contains(s));
        }

        // Expand — apply Include before ordering (per D-03)
        if (!string.IsNullOrWhiteSpace(query.Expand))
        {
            var expand = query.Expand.Split(',', StringSplitOptions.TrimEntries);
            if (expand.Contains("subject"))
                q = q.Include(e => e.Subject);
            if (expand.Contains("semester"))
                q = q.Include(e => e.Semester);
        }

        // Sort — per D-01
        var sortFields = new Dictionary<string, (Func<IQueryable<Course>, IOrderedQueryable<Course>> asc, Func<IQueryable<Course>, IOrderedQueryable<Course>> desc)>
        {
            { "id", (q => q.OrderBy(e => e.Id), q => q.OrderByDescending(e => e.Id)) },
            { "code", (q => q.OrderBy(e => e.Code), q => q.OrderByDescending(e => e.Code)) },
            { "instructor", (q => q.OrderBy(e => e.Instructor), q => q.OrderByDescending(e => e.Instructor)) },
            { "room", (q => q.OrderBy(e => e.Room), q => q.OrderByDescending(e => e.Room)) },
            { "maxstudents", (q => q.OrderBy(e => e.MaxStudents), q => q.OrderByDescending(e => e.MaxStudents)) },
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

        return new PagedResult<CourseResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CourseResponse?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<CourseResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : null;
        var entity = await _unitOfWork.Courses.GetByIdAsync(id, includes);
        if (entity == null) return null;

        var expandArr = includes ?? [];
        return entity.ToBusinessModel().ToResponseDto(expandArr);
    }

    public async Task<CourseResponse> AddAsync(CourseRequest request)
    {
        var entity = request.ToEntity();
        var created = _unitOfWork.Courses.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return created.ToBusinessModel().ToResponseDto();
    }

    public async Task<CourseResponse?> UpdateAsync(int id, CourseRequest request)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id);
        if (entity == null) return null;

        request.UpdateEntity(entity);
        _unitOfWork.Courses.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel().ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Courses.Delete(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
