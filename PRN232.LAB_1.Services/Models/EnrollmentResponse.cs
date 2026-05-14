namespace PRN232.LAB_1.Services.Models;

public class EnrollmentResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public StudentResponse? Student { get; set; }
    public CourseResponse? Course { get; set; }
}
