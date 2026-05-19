using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class DepartmentResponse
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDepartmentRequest
{
    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DepartmentName { get; set; } = string.Empty;
}

public class UpdateDepartmentRequest
{
    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DepartmentName { get; set; } = string.Empty;
}
