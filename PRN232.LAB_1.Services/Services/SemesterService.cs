using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Helpers;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SemesterResponse>> GetAllAsync()
    {
        var entities = await _unitOfWork.Semesters.GetAllAsync();
        return entities.Select(e => e.ToBusinessModel()).ToResponseDtoList();
    }

    public async Task<PagedResult<SemesterResponse>> GetAllAsync(PagedQuery query)
    {
        var q = _unitOfWork.Semesters.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(e => e.Code.ToLower().Contains(s) || e.Name.ToLower().Contains(s));
        }

        // Expand stub — Semester has no forward navigation properties to expand
        // (Courses is a backward navigation from Course → Semester)

        // Sort — per D-01, multi-field sort with known fields dictionary
        var sortFields = new Dictionary<string, (Func<IQueryable<Semester>, IOrderedQueryable<Semester>> asc, Func<IQueryable<Semester>, IOrderedQueryable<Semester>> desc)>
        {
            { "id", (q => q.OrderBy(e => e.Id), q => q.OrderByDescending(e => e.Id)) },
            { "code", (q => q.OrderBy(e => e.Code), q => q.OrderByDescending(e => e.Code)) },
            { "name", (q => q.OrderBy(e => e.Name), q => q.OrderByDescending(e => e.Name)) },
            { "startdate", (q => q.OrderBy(e => e.StartDate), q => q.OrderByDescending(e => e.StartDate)) },
            { "enddate", (q => q.OrderBy(e => e.EndDate), q => q.OrderByDescending(e => e.EndDate)) },
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

        return new PagedResult<SemesterResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SemesterResponse?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<SemesterResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : null;
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id, includes);
        return entity?.ToBusinessModel().ToResponseDto();
    }

    public async Task<SemesterResponse> AddAsync(SemesterRequest request)
    {
        var entity = request.ToEntity();
        var created = _unitOfWork.Semesters.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return created.ToBusinessModel().ToResponseDto();
    }

    public async Task<SemesterResponse?> UpdateAsync(int id, SemesterRequest request)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (entity == null) return null;

        request.UpdateEntity(entity);
        _unitOfWork.Semesters.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel().ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Semesters.Delete(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
