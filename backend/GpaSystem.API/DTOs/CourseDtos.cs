using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public byte CreditHours { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCourseRequest
{
    [StringLength(20, MinimumLength = 2)]
    public string? CourseCode { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string CourseTitle { get; set; } = string.Empty;

    [Range(1, byte.MaxValue)]
    public byte CreditHours { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateCourseRequest
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string CourseCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string CourseTitle { get; set; } = string.Empty;

    [Range(1, byte.MaxValue)]
    public byte CreditHours { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
