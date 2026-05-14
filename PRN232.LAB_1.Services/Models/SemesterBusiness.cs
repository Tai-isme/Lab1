namespace PRN232.LAB_1.Services.Models;

public class SemesterBusiness
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<CourseBusiness> Courses { get; set; } = [];
}
