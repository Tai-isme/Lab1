using System.ComponentModel.DataAnnotations;

namespace PRN232.LAB_1.Services.Models;

public class EnrollmentRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Grade { get; set; }
}
