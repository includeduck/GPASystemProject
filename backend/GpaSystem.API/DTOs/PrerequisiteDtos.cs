using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class PrerequisiteResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int PrerequisiteCourseId { get; set; }
    public string PrerequisiteCourseCode { get; set; } = string.Empty;
    public string PrerequisiteCourseTitle { get; set; } = string.Empty;
}

public class AddPrerequisiteRequest
{
    [Range(1, int.MaxValue)]
    public int PrerequisiteCourseId { get; set; }
}
