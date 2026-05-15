namespace PRN232.LAB_1.Services.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies multi-field sorting. Returns ordered query.
    /// </summary>
    /// <param name="source">The queryable source.</param>
    /// <param name="sort">Comma-separated sort fields, e.g. "name,-id". -prefix = descending.</param>
    /// <param name="knownFields">Dictionary of lowercase field name → (propertyLambda, isDescending).</param>
    /// <param name="defaultOrderKey">Default sort key when sort is empty/null.</param>
    public static IOrderedQueryable<T> ApplyMultiFieldSort<T>(
        this IQueryable<T> source,
        string? sort,
        Dictionary<string, (LambdaExpression propertySelector, bool desc)> knownFields,
        string defaultOrderKey = "id") where T : class
    {
        if (string.IsNullOrWhiteSpace(sort) || !knownFields.Any())
        {
            if (knownFields.TryGetValue(defaultOrderKey, out var def))
                return def.desc ? source.OrderByDescending(def.propertySelector) : source.OrderBy(def.propertySelector);
            return source.OrderBy(e => EF.Property<int>(e, "Id"));
        }

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<T>? ordered = null;

        foreach (var field in fields)
        {
            bool desc = field.StartsWith('-');
            var key = desc ? field[1..].Trim().ToLowerInvariant() : field.Trim().ToLowerInvariant();

            if (!knownFields.TryGetValue(key, out var config))
                continue;

            if (ordered == null)
            {
                ordered = desc
                    ? source.OrderByDescending(config.propertySelector)
                    : source.OrderBy(config.propertySelector);
            }
            else
            {
                ordered = desc
                    ? ordered.ThenByDescending(config.propertySelector)
                    : ordered.ThenBy(config.propertySelector);
            }
        }

        return ordered ?? source.OrderBy(e => EF.Property<int>(e, "Id"));
    }

    /// <summary>
    /// Post-mapping field selection: nulls out properties NOT in the requested fields list.
    /// Always preserves the Id property. Case-insensitive matching.
    /// Operates on IEnumerable (in-memory after ToListAsync).
    /// Per D-02.
    /// </summary>
    public static IEnumerable<T> ApplyFieldSelection<T>(
        this IEnumerable<T> items,
        string? fields) where T : class
    {
        if (string.IsNullOrWhiteSpace(fields))
            return items;

        var requestedFields = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.ToLowerInvariant())
            .ToHashSet();

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        foreach (var item in items)
        {
            foreach (var prop in props)
            {
                if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue; // Always preserve Id

                if (!requestedFields.Contains(prop.Name.ToLowerInvariant()))
                {
                    var defaultValue = prop.PropertyType.IsValueType
                        ? Activator.CreateInstance(prop.PropertyType)
                        : null;
                    prop.SetValue(item, defaultValue);
                }
            }
        }

        return items;
    }
}
