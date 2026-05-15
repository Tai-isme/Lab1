namespace PRN232.LAB_1.Services.Models;

public class PagedQuery
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }
}
