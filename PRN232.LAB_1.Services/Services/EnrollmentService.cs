using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Helpers;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class EnrollmentService
    : GenericService<Enrollment, EnrollmentBusiness, EnrollmentRequest, EnrollmentResponse>,
      IEnrollmentService
{
    public EnrollmentService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override EnrollmentBusiness MapToBusiness(Enrollment entity)
        => entity.ToBusinessModel();

    protected override EnrollmentResponse MapToResponse(EnrollmentBusiness business, string[] expand)
        => business.ToResponseDto(expand);   // EnrollmentMapper has expand-aware overload

    protected override Enrollment MapToEntity(EnrollmentRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(EnrollmentRequest request, Enrollment entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Enrollment> ApplySearch(IQueryable<Enrollment> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Status.ToLower().Contains(s)
                         || (e.Grade != null && e.Grade.ToLower().Contains(s)));
    }

    // ── Expand hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Enrollment> ApplyExpand(IQueryable<Enrollment> q, string[] expand)
    {
        if (expand.Contains("student"))
            q = q.Include(e => e.Student);
        if (expand.Contains("course"))
            q = q.Include(e => e.Course);
        return q;
    }

    // ── Override expand methods to use entity-level ToResponseDto ────────────

    public override async Task<EnrollmentResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        var q = UnitOfWork.Repository<Enrollment>().GetQueryable();
        if (includes.Length > 0)
            q = ApplyExpand(q, includes);
        var entity = await q.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return null;
        return entity.ToResponseDto(includes);
    }

    public override async Task<PagedResult<EnrollmentResponse>> GetAllAsync(PagedQuery query)
    {
        var q = UnitOfWork.Repository<Enrollment>().GetQueryable();

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

        return new PagedResult<EnrollmentResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>> Asc,
                                           Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>> Desc)>
        GetSortFields() => new()
        {
            { "id",             (q => q.OrderBy(e => e.Id),             q => q.OrderByDescending(e => e.Id)) },
            { "enrollmentdate", (q => q.OrderBy(e => e.EnrollmentDate), q => q.OrderByDescending(e => e.EnrollmentDate)) },
            { "status",         (q => q.OrderBy(e => e.Status),         q => q.OrderByDescending(e => e.Status)) },
            { "grade",          (q => q.OrderBy(e => e.Grade!),         q => q.OrderByDescending(e => e.Grade!)) },
        };
}
