using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class EnrollmentService : IEnrollmentService
{
    private const decimal DefaultPassingCutoff = 50m;

    private readonly GpaSystemDbContext _db;
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly ICourseOfferingRepository _offerings;
    private readonly ISemesterRepository _semesters;
    private readonly ICoursePrerequisiteRepository _prerequisites;

    public EnrollmentService(
        GpaSystemDbContext db,
        IEnrollmentRepository enrollments,
        IStudentRepository students,
        ICourseOfferingRepository offerings,
        ISemesterRepository semesters,
        ICoursePrerequisiteRepository prerequisites)
    {
        _db = db;
        _enrollments = enrollments;
        _students = students;
        _offerings = offerings;
        _semesters = semesters;
        _prerequisites = prerequisites;
    }

    public async Task<List<EnrollmentResponse>> GetForStudentAsync(int studentId)
    {
        await FindActiveStudentAsync(studentId);
        var enrollments = await _enrollments.GetByStudentAsync(studentId);
        return enrollments.Select(Map).ToList();
    }

    public async Task<List<AvailableOfferingResponse>> GetAvailableOfferingsAsync(int studentId, int? semesterId = null)
    {
        var student = await FindActiveStudentAsync(studentId);
        var resolvedSemesterId = semesterId ?? (await _semesters.GetCurrentAsync())?.SemesterId;
        var offerings = await _offerings.GetAllAsync(resolvedSemesterId);
        var passingCutoff = await GetPassingCutoffAsync();
        var availableOfferings = new List<AvailableOfferingResponse>();

        foreach (var offering in offerings.Where(o => o.Status == "ACTIVE"))
        {
            availableOfferings.Add(await BuildAvailableOfferingAsync(student, offering, passingCutoff));
        }

        return availableOfferings;
    }

    public async Task<EnrollmentResponse> EnrollAsync(CreateEnrollmentRequest request)
    {
        var student = await FindActiveStudentAsync(request.StudentId);
        var offering = await FindActiveOfferingAsync(request.OfferingId);
        var passingCutoff = await GetPassingCutoffAsync();

        if (await _enrollments.ExistsAsync(student.StudentId, offering.OfferingId))
        {
            throw ApiException.Conflict("Student is already enrolled in this course offering.");
        }

        if (await IsOfferingFullAsync(offering))
        {
            throw ApiException.Conflict("Course offering has reached maximum capacity.");
        }

        var missingPrerequisites = await GetMissingPrerequisitesAsync(
            student.StudentId,
            offering.CourseId,
            passingCutoff);

        if (missingPrerequisites.Count > 0)
        {
            var missingCourses = string.Join(", ", missingPrerequisites.Select(p => p.CourseCode));
            throw ApiException.Conflict($"Missing prerequisite courses: {missingCourses}.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var enrollment = new Enrollment
        {
            StudentId = student.StudentId,
            Student = student,
            OfferingId = offering.OfferingId,
            CourseOffering = offering,
            EnrollmentDate = DateTime.UtcNow,
            Status = "ENROLLED",
            IsRepeated = false
        };

        try
        {
            await _enrollments.AddAsync(enrollment);
            await _enrollments.SaveChangesAsync();
            await _offerings.ReconcileCurrentEnrollmentAsync(offering.OfferingId);
            await _offerings.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync();
            throw TranslateEnrollmentUpdateException(exception);
        }

        var savedEnrollment = await _enrollments.GetByIdAsync(enrollment.EnrollmentId)
            ?? throw ApiException.NotFound("Enrollment was not found after creation.");

        return Map(savedEnrollment);
    }

    private async Task<AvailableOfferingResponse> BuildAvailableOfferingAsync(
        Student student,
        CourseOffering offering,
        decimal passingCutoff)
    {
        var alreadyEnrolled = await _enrollments.ExistsAsync(student.StudentId, offering.OfferingId);
        var isFull = await IsOfferingFullAsync(offering);
        var missingPrerequisites = await GetMissingPrerequisitesAsync(
            student.StudentId,
            offering.CourseId,
            passingCutoff);

        var blockedReason = BuildBlockedReason(alreadyEnrolled, isFull, missingPrerequisites);

        return new AvailableOfferingResponse
        {
            Offering = CourseOfferingService.Map(offering),
            IsAlreadyEnrolled = alreadyEnrolled,
            HasCapacity = !isFull,
            HasPrerequisites = missingPrerequisites.Count == 0,
            CanEnroll = blockedReason is null,
            BlockedReason = blockedReason,
            MissingPrerequisites = missingPrerequisites
        };
    }

    private async Task<Student> FindActiveStudentAsync(int id)
    {
        var student = await _students.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Student was not found.");

        if (student.Status != "ACTIVE" || !student.User.IsActive)
        {
            throw ApiException.BadRequest("Only active students can enroll in courses.");
        }

        return student;
    }

    private async Task<CourseOffering> FindActiveOfferingAsync(int id)
    {
        var offering = await _offerings.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Course offering was not found.");

        if (offering.Status != "ACTIVE")
        {
            throw ApiException.BadRequest("Only active course offerings can accept enrollments.");
        }

        return offering;
    }

    private async Task<bool> IsOfferingFullAsync(CourseOffering offering)
    {
        var activeEnrollmentCount = await _offerings.CountActiveEnrollmentsAsync(offering.OfferingId);
        return activeEnrollmentCount >= offering.MaxCapacity ||
               offering.CurrentEnrollment >= offering.MaxCapacity;
    }

    private async Task<List<MissingPrerequisiteResponse>> GetMissingPrerequisitesAsync(
        int studentId,
        int courseId,
        decimal passingCutoff)
    {
        var prerequisites = await _prerequisites.GetForCourseAsync(courseId);
        var missingPrerequisites = new List<MissingPrerequisiteResponse>();

        foreach (var prerequisite in prerequisites)
        {
            var hasPassed = await _db.CourseGrades.AnyAsync(grade =>
                grade.Percentage >= passingCutoff &&
                grade.Enrollment.StudentId == studentId &&
                grade.Enrollment.Status == "COMPLETED" &&
                grade.Enrollment.CourseOffering.CourseId == prerequisite.PrerequisiteCourseId);

            if (!hasPassed)
            {
                missingPrerequisites.Add(new MissingPrerequisiteResponse
                {
                    CourseId = prerequisite.PrerequisiteCourseId,
                    CourseCode = prerequisite.PrerequisiteCourse.CourseCode,
                    CourseTitle = prerequisite.PrerequisiteCourse.CourseTitle
                });
            }
        }

        return missingPrerequisites;
    }

    private async Task<decimal> GetPassingCutoffAsync()
    {
        var configuredValue = await _db.Configurations
            .Where(configuration => configuration.ConfigKey == "pass_fail_cutoff")
            .Select(configuration => configuration.ConfigValue)
            .FirstOrDefaultAsync();

        return decimal.TryParse(configuredValue, out var passingCutoff)
            ? passingCutoff
            : DefaultPassingCutoff;
    }

    private static string? BuildBlockedReason(
        bool alreadyEnrolled,
        bool isFull,
        IReadOnlyCollection<MissingPrerequisiteResponse> missingPrerequisites)
    {
        if (alreadyEnrolled)
        {
            return "Already enrolled";
        }

        if (isFull)
        {
            return "Course full";
        }

        if (missingPrerequisites.Count > 0)
        {
            return "Missing prerequisites";
        }

        return null;
    }

    private static ApiException TranslateEnrollmentUpdateException(DbUpdateException exception)
    {
        var message = exception.GetBaseException().Message;

        if (message.Contains("maximum capacity", StringComparison.OrdinalIgnoreCase))
        {
            return ApiException.Conflict("Course offering has reached maximum capacity.");
        }

        if (message.Contains("UQ_StudentOffering", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("unique", StringComparison.OrdinalIgnoreCase))
        {
            return ApiException.Conflict("Student is already enrolled in this course offering.");
        }

        return new ApiException(
            StatusCodes.Status500InternalServerError,
            "Enrollment could not be saved.");
    }

    private static EnrollmentResponse Map(Enrollment enrollment)
    {
        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            StudentNumber = enrollment.Student.StudentNumber,
            StudentName = enrollment.Student.FullName,
            OfferingId = enrollment.OfferingId,
            CourseId = enrollment.CourseOffering.CourseId,
            CourseCode = enrollment.CourseOffering.Course.CourseCode,
            CourseTitle = enrollment.CourseOffering.Course.CourseTitle,
            CreditHours = enrollment.CourseOffering.Course.CreditHours,
            SemesterId = enrollment.CourseOffering.SemesterId,
            SemesterName = enrollment.CourseOffering.Semester.SemesterName,
            InstructorId = enrollment.CourseOffering.InstructorId,
            InstructorName = enrollment.CourseOffering.Instructor.FullName,
            EnrollmentDate = enrollment.EnrollmentDate,
            Status = enrollment.Status,
            IsRepeated = enrollment.IsRepeated
        };
    }
}
