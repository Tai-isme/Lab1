namespace PRN232.LAB_1.Services.Models;

/// <summary>
/// Generic paginated result model returned by collection endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PagedResult<T>
{
    /// <summary>The items for the current page.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>The current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>The number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>The total number of items across all pages.</summary>
    public int TotalItems { get; set; }

    /// <summary>The total number of pages.</summary>
    public int TotalPages { get; set; }
}
