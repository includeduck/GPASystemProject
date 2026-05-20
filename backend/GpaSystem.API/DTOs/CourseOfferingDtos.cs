using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class CourseOfferingResponse
{
    public int OfferingId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public byte CreditHours { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public bool IsCurrentSemester { get; set; }
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int SeatsAvailable { get; set; }
    public bool IsFull { get; set; }
    public bool IsGradeFinalized { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateCourseOfferingRequest
{
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }

    [Range(1, int.MaxValue)]
    public int InstructorId { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }

    [RegularExpression("ACTIVE|COMPLETED|CANCELLED")]
    public string Status { get; set; } = "ACTIVE";
}

public class UpdateCourseOfferingRequest
{
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }

    [Range(1, int.MaxValue)]
    public int InstructorId { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }

    [RegularExpression("ACTIVE|COMPLETED|CANCELLED")]
    public string Status { get; set; } = "ACTIVE";
}
