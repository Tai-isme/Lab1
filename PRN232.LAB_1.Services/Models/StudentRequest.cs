using System.ComponentModel.DataAnnotations;

namespace PRN232.LAB_1.Services.Models;

public class StudentRequest
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
}
