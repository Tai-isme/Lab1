using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
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
