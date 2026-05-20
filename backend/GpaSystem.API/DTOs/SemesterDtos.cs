using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
}

public class CreateSemesterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string SemesterName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsCurrent { get; set; }
}

public class UpdateSemesterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string SemesterName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsCurrent { get; set; }
}
