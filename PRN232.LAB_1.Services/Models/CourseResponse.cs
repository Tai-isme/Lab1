namespace PRN232.LAB_1.Services.Models;

public class CourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public int SemesterId { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public string Schedule { get; set; } = string.Empty;
    public SubjectResponse? Subject { get; set; }
    public SemesterResponse? Semester { get; set; }
}
