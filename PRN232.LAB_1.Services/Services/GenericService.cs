using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Services.Helpers;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public abstract class GenericService<TEntity, TBusiness, TRequest, TResponse>
    : IGenericService<TResponse, TRequest>
    where TEntity : class
    where TBusiness : class
    where TRequest : class
    where TResponse : class
{
    protected readonly IUnitOfWork UnitOfWork;

    protected GenericService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    // ── Abstract mapping hooks (D-07, D-08) ──────────────────────────────────

    /// <summary>Map a TEntity (EF entity) to TBusiness (business model).</summary>
    protected abstract TBusiness MapToBusiness(TEntity entity);

    /// <summary>Map TBusiness to TResponse with optional expand context (D-08).</summary>
    protected abstract TResponse MapToResponse(TBusiness business, string[] expand);

    /// <summary>Map TBusiness to TResponse with no expand context.</summary>
    protected virtual TResponse MapToResponse(TBusiness business)
        => MapToResponse(business, Array.Empty<string>());

    /// <summary>Map TRequest to a new TEntity for Add.</summary>
    protected abstract TEntity MapToEntity(TRequest request);

    /// <summary>Apply TRequest fields onto an existing TEntity for Update.</summary>
    protected abstract void UpdateEntityFromRequest(TRequest request, TEntity entity);

    // ── Abstract/virtual query hooks (D-03) ──────────────────────────────────

    /// <summary>
    /// Apply entity-specific search filter to the queryable.
    /// Base returns q unchanged (no-op) when not overridden.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> q, string search)
        => q;

    /// <summary>
    /// Apply EF Include statements for navigation property expansion.
    /// Base returns q unchanged (no-op) when not overridden.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyExpand(IQueryable<TEntity> q, string[] expand)
        => q;

    /// <summary>
    /// Return the sort-field dictionary used by ApplyMultiFieldSort.
    /// Keys must be lowercase. Base returns an empty dictionary (sorts by natural order).
    /// </summary>
    protected virtual Dictionary<string, (Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> Asc,
                                          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> Desc)>
        GetSortFields()
        => new();

    // ── IGenericService<TResponse, TRequest> implementation ──────────────────

    public virtual async Task<List<TResponse>> GetAllAsync()
    {
        var entities = await UnitOfWork.Repository<TEntity>().GetAllAsync();
        return entities
            .Select(e => MapToResponse(MapToBusiness(e)))
            .ToList();
    }

    public virtual async Task<PagedResult<TResponse>> GetAllAsync(PagedQuery query)
    {
        var q = UnitOfWork.Repository<TEntity>().GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = ApplySearch(q, query.Search);

        // Expand (include navigation props before count/order)
        var expandArr = !string.IsNullOrWhiteSpace(query.Expand)
            ? query.Expand.Split(',', StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        if (expandArr.Length > 0)
            q = ApplyExpand(q, expandArr);

        // Sort
        var sortFields = GetSortFields();
        var ordered = q.ApplyMultiFieldSort(query.Sort, sortFields);

        // Count
        var totalItems = await ordered.CountAsync();

        // Page
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pagedEntities = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map + field selection
        var items = pagedEntities
            .Select(e => MapToResponse(MapToBusiness(e), expandArr))
            .ToList()
            .ApplyFieldSelection(query.Fields)
            .ToList();

        return new PagedResult<TResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public virtual async Task<TResponse?> GetByIdAsync(int id)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id);
        if (entity == null) return null;
        return MapToResponse(MapToBusiness(entity));
    }

    public virtual async Task<TResponse?> GetByIdAsync(int id, string? expand)
    {
        var includes = !string.IsNullOrWhiteSpace(expand)
            ? expand.Split(',', StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        var q = UnitOfWork.Repository<TEntity>().GetQueryable();
        if (includes.Length > 0)
            q = ApplyExpand(q, includes);
        var entity = await q.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        if (entity == null) return null;
        return MapToResponse(MapToBusiness(entity), includes);
    }

    public virtual async Task<TResponse> AddAsync(TRequest request)
    {
        var entity = MapToEntity(request);
        var created = await UnitOfWork.Repository<TEntity>().AddAsync(entity);
        await UnitOfWork.SaveChangesAsync();
        return MapToResponse(MapToBusiness(created));
    }

    public virtual async Task<TResponse?> UpdateAsync(int id, TRequest request)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id);
        if (entity == null) return null;
        UpdateEntityFromRequest(request, entity);
        await UnitOfWork.Repository<TEntity>().UpdateAsync(entity);
        await UnitOfWork.SaveChangesAsync();
        return MapToResponse(MapToBusiness(entity));
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await UnitOfWork.Repository<TEntity>().GetByIdAsync(id);
        if (entity == null) return false;
        await UnitOfWork.Repository<TEntity>().DeleteAsync(entity);
        await UnitOfWork.SaveChangesAsync();
        return true;
    }
}
