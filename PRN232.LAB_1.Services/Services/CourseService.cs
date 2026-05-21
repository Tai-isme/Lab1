using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Helpers;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class CourseService
    : GenericService<Course, CourseBusiness, CourseRequest, CourseResponse>,
      ICourseService
{
    public CourseService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override CourseBusiness MapToBusiness(Course entity)
        => entity.ToBusinessModel();

    protected override CourseResponse MapToResponse(CourseBusiness business, string[] expand)
        => business.ToResponseDto(expand);   // CourseMapper has expand-aware overload

    protected override Course MapToEntity(CourseRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(CourseRequest request, Course entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Course> ApplySearch(IQueryable<Course> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Code.ToLower().Contains(s)
                         || e.Instructor.ToLower().Contains(s)
                         || e.Room.ToLower().Contains(s)
                         || e.Schedule.ToLower().Contains(s));
    }

    // ── Expand hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Course> ApplyExpand(IQueryable<Course> q, string[] expand)
    {
        if (expand.Contains("subject"))
            q = q.Include(e => e.Subject);
        if (expand.Contains("semester"))
            q = q.Include(e => e.Semester);
        return q;
    }

    // ── Override expand methods to use entity-level ToResponseDto ────────────

    public override async Task<CourseResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        var q = UnitOfWork.Repository<Course>().GetQueryable();
        if (includes.Length > 0)
            q = ApplyExpand(q, includes);
        var entity = await q.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return null;
        return entity.ToResponseDto(includes);
    }

    public override async Task<PagedResult<CourseResponse>> GetAllAsync(PagedQuery query)
    {
        var q = UnitOfWork.Repository<Course>().GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = ApplySearch(q, query.Search);

        var expandArr = !string.IsNullOrWhiteSpace(query.Expand)
            ? query.Expand.Split(',', StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        if (expandArr.Length > 0)
            q = ApplyExpand(q, expandArr);

        var sortFields = GetSortFields();
        var ordered = q.ApplyMultiFieldSort(query.Sort, sortFields);

        var totalItems = await ordered.CountAsync();

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pagedEntities = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = pagedEntities
            .Select(e => e.ToResponseDto(expandArr))
            .ToList()
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

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Course>, IOrderedQueryable<Course>> Asc,
                                           Func<IQueryable<Course>, IOrderedQueryable<Course>> Desc)>
        GetSortFields() => new()
        {
            { "id",          (q => q.OrderBy(e => e.Id),          q => q.OrderByDescending(e => e.Id)) },
            { "code",        (q => q.OrderBy(e => e.Code),        q => q.OrderByDescending(e => e.Code)) },
            { "instructor",  (q => q.OrderBy(e => e.Instructor),  q => q.OrderByDescending(e => e.Instructor)) },
            { "room",        (q => q.OrderBy(e => e.Room),        q => q.OrderByDescending(e => e.Room)) },
            { "maxstudents", (q => q.OrderBy(e => e.MaxStudents), q => q.OrderByDescending(e => e.MaxStudents)) },
        };
}
