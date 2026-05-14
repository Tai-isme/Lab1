namespace PRN232.LAB_1.Services.Models;

public class SubjectBusiness
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Credits { get; set; }
    public List<CourseBusiness> Courses { get; set; } = [];
}
