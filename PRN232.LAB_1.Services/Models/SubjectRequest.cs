using System.ComponentModel.DataAnnotations;

namespace PRN232.LAB_1.Services.Models;

public class SubjectRequest
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credits { get; set; }
}
