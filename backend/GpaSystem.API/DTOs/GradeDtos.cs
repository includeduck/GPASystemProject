using System;
using System.Collections.Generic;

namespace GpaSystem.API.DTOs;

public class GradeComponentResponse
{
    public int ComponentId { get; set; }
    public int OfferingId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal MaxPoints { get; set; }
    public byte SortOrder { get; set; }
}

public class CreateGradeComponentRequest
{
    public string ComponentName { get; set; } = string.Empty;
    public decimal MaxPoints { get; set; }
    public byte SortOrder { get; set; }
}

public class UpdateGradeComponentRequest
{
    public string ComponentName { get; set; } = string.Empty;
    public decimal MaxPoints { get; set; }
    public byte SortOrder { get; set; }
}

public class GradeEntryResponse
{
    public int GradeEntryId { get; set; }
    public int EnrollmentId { get; set; }
    public int ComponentId { get; set; }
    public decimal ObtainedMarks { get; set; }
    public int RecordedBy { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class RecordGradeEntryRequest
{
    public int EnrollmentId { get; set; }
    public int ComponentId { get; set; }
    public decimal ObtainedMarks { get; set; }
}

public class RosterGradeResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public List<GradeEntryResponse> Entries { get; set; } = new();
    public decimal? TotalObtained { get; set; }
    public decimal? MaxPossible { get; set; }
    public decimal? Percentage { get; set; }
    public string? LetterGrade { get; set; }
    public decimal? GradePoints { get; set; }
    public string EnrollmentStatus { get; set; } = string.Empty;
}

public class GradingPolicyResponse
{
    public int PolicyId { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal GradePoint { get; set; }
    public bool IsActive { get; set; }
    public DateOnly EffectiveFrom { get; set; }
}

public class UpdateGradingPolicyRequest
{
    public int? PolicyId { get; set; } // Null if creating a new range boundary
    public string LetterGrade { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal GradePoint { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public class FinalizeGradesRequest
{
    public bool Force { get; set; } = false;
}

public class CourseGradeResponse
{
    public int GradeId { get; set; }
    public int EnrollmentId { get; set; }
    public decimal TotalObtained { get; set; }
    public decimal MaxPossible { get; set; }
    public decimal Percentage { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal GradePoints { get; set; }
    public bool IsRepeatedAttempt { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class StudentCourseGradeResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public decimal TotalObtained { get; set; }
    public decimal MaxPossible { get; set; }
    public decimal Percentage { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal GradePoints { get; set; }
    public bool IsRepeatedAttempt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SemesterResultResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public decimal CGPA { get; set; }
    public int CreditsAttempted { get; set; }
    public int CreditsEarned { get; set; }
    public List<StudentCourseGradeResponse> Courses { get; set; } = new();
}

public class StudentDashboardResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public decimal CGPA { get; set; }
    public int TotalCreditsAttempted { get; set; }
    public int TotalCreditsEarned { get; set; }
    public List<SemesterResultResponse> Semesters { get; set; } = new();
}
