using System.ComponentModel.DataAnnotations;

namespace GpaSystem.API.DTOs;

public class MissingPrerequisiteResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
}

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int OfferingId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public byte CreditHours { get; set; }
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRepeated { get; set; }
}

public class AvailableOfferingResponse
{
    public CourseOfferingResponse Offering { get; set; } = new();
    public bool IsAlreadyEnrolled { get; set; }
    public bool HasCapacity { get; set; }
    public bool HasPrerequisites { get; set; }
    public bool CanEnroll { get; set; }
    public string? BlockedReason { get; set; }
    public List<MissingPrerequisiteResponse> MissingPrerequisites { get; set; } = new();
}

public class CreateEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int OfferingId { get; set; }
}
