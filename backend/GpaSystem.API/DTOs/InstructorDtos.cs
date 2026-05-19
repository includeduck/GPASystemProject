using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class InstructorResponse
{
    public int InstructorId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInstructorRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public DateOnly? HireDate { get; set; }
}

public class UpdateInstructorRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public DateOnly? HireDate { get; set; }
}

public class CreateInstructorResponse
{
    public InstructorResponse Instructor { get; set; } = new();
    public TemporaryCredentialsResponse Credentials { get; set; } = new();
}
