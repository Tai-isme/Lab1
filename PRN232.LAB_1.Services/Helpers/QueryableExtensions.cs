namespace PRN232.LAB_1.Services.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies multi-field sorting. Returns ordered query.
    /// Uses reflection for ThenBy/ThenByDescending to handle generic TKey at runtime.
    /// </summary>
    /// <param name="source">The queryable source.</param>
    /// <param name="sort">Comma-separated sort fields, e.g. "name,-id". -prefix = descending.</param>
    /// <param name="sortFunctions">Dictionary of lowercase field name → (ascending function, descending function).</param>
    /// <param name="defaultOrderKey">Default sort key when sort is empty/null.</param>
    public static IOrderedQueryable<T> ApplyMultiFieldSort<T>(
        this IQueryable<T> source,
        string? sort,
        Dictionary<string, (Func<IQueryable<T>, IOrderedQueryable<T>> asc, Func<IQueryable<T>, IOrderedQueryable<T>> desc)> sortFunctions,
        string defaultOrderKey = "id") where T : class
    {
        if (string.IsNullOrWhiteSpace(sort) || !sortFunctions.Any())
        {
            if (sortFunctions.TryGetValue(defaultOrderKey, out var def))
                return def.asc(source);
            return source.OrderBy(e => EF.Property<int>(e, "Id"));
        }

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<T>? ordered = null;

        foreach (var field in fields)
        {
            bool desc = field.StartsWith('-');
            var key = desc ? field[1..].Trim().ToLowerInvariant() : field.Trim().ToLowerInvariant();

            if (!sortFunctions.TryGetValue(key, out var funcs))
                continue;

            if (ordered == null)
            {
                ordered = desc ? funcs.desc(source) : funcs.asc(source);
            }
            else
            {
                // Extract key selector from the asc function's lambda
                var lambda = (LambdaExpression)((MethodCallExpression)funcs.asc(source).Expression).Arguments[1];
                var keySelector = lambda.Body.NodeType == ExpressionType.Convert
                    ? Expression.Lambda(((UnaryExpression)lambda.Body).Operand, lambda.Parameters)
                    : lambda;

                var methodName = desc ? "ThenByDescending" : "ThenBy";
                var thenByMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), keySelector.ReturnType);

                ordered = (IOrderedQueryable<T>)thenByMethod.Invoke(null, new object[] { ordered, keySelector })!;
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
