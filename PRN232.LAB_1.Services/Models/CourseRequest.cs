using System.ComponentModel.DataAnnotations;

namespace PRN232.LAB_1.Services.Models;

public class CourseRequest
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int SemesterId { get; set; }

    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = string.Empty;

    [StringLength(50)]
    public string Room { get; set; } = string.Empty;

    [Range(1, 200)]
    public int MaxStudents { get; set; }

    [StringLength(200)]
    public string Schedule { get; set; } = string.Empty;
}
