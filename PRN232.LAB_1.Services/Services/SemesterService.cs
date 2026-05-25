using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class SemesterService
    : GenericService<Semester, SemesterBusiness, SemesterRequest, SemesterResponse>,
      ISemesterService
{
    public SemesterService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override SemesterBusiness MapToBusiness(Semester entity)
        => entity.ToBusinessModel();

    protected override SemesterResponse MapToResponse(SemesterBusiness business, string[] expand)
        => business.ToResponseDto(expand);

    protected override Semester MapToEntity(SemesterRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(SemesterRequest request, Semester entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Semester> ApplySearch(IQueryable<Semester> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Code.ToLower().Contains(s) || e.Name.ToLower().Contains(s));
    }

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Semester>, IOrderedQueryable<Semester>> Asc,
                                           Func<IQueryable<Semester>, IOrderedQueryable<Semester>> Desc)>
        GetSortFields() => new()
        {
            { "id",        (q => q.OrderBy(e => e.Id),        q => q.OrderByDescending(e => e.Id)) },
            { "code",      (q => q.OrderBy(e => e.Code),      q => q.OrderByDescending(e => e.Code)) },
            { "name",      (q => q.OrderBy(e => e.Name),      q => q.OrderByDescending(e => e.Name)) },
            { "startdate", (q => q.OrderBy(e => e.StartDate), q => q.OrderByDescending(e => e.StartDate)) },
            { "enddate",   (q => q.OrderBy(e => e.EndDate),   q => q.OrderByDescending(e => e.EndDate)) },
        };

    protected override IQueryable<Semester> ApplyExpand(IQueryable<Semester> q, string[] expand)
    {
        if (expand.Contains("courses"))
            q = q.Include(e => e.Courses);
        return q;
    }
}
