using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class SubjectService
    : GenericService<Subject, SubjectBusiness, SubjectRequest, SubjectResponse>,
      ISubjectService
{
    public SubjectService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override SubjectBusiness MapToBusiness(Subject entity)
        => entity.ToBusinessModel();

    protected override SubjectResponse MapToResponse(SubjectBusiness business, string[] expand)
        => business.ToResponseDto();   // Subject has no forward navigation properties

    protected override Subject MapToEntity(SubjectRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(SubjectRequest request, Subject entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Subject> ApplySearch(IQueryable<Subject> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Code.ToLower().Contains(s)
                         || e.Name.ToLower().Contains(s)
                         || e.Description.ToLower().Contains(s));
    }

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Subject>, IOrderedQueryable<Subject>> Asc,
                                           Func<IQueryable<Subject>, IOrderedQueryable<Subject>> Desc)>
        GetSortFields() => new()
        {
            { "id",      (q => q.OrderBy(e => e.Id),      q => q.OrderByDescending(e => e.Id)) },
            { "code",    (q => q.OrderBy(e => e.Code),    q => q.OrderByDescending(e => e.Code)) },
            { "name",    (q => q.OrderBy(e => e.Name),    q => q.OrderByDescending(e => e.Name)) },
            { "credits", (q => q.OrderBy(e => e.Credits), q => q.OrderByDescending(e => e.Credits)) },
        };

    // ApplyExpand not overridden — Subject has no forward navigation properties
}
