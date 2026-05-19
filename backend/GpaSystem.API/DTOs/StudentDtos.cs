using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class StudentResponse
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }
}

public class UpdateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    [RegularExpression("ACTIVE|INACTIVE|GRADUATED")]
    public string Status { get; set; } = "ACTIVE";
}

public class CreateStudentResponse
{
    public StudentResponse Student { get; set; } = new();
    public TemporaryCredentialsResponse Credentials { get; set; } = new();
}
