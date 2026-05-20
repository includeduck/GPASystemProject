using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class CourseOfferingService : ICourseOfferingService
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.Ordinal)
    {
        "ACTIVE",
        "COMPLETED",
        "CANCELLED"
    };

    private readonly ICourseOfferingRepository _offerings;
    private readonly ICourseRepository _courses;
    private readonly ISemesterRepository _semesters;
    private readonly IInstructorRepository _instructors;

    public CourseOfferingService(
        ICourseOfferingRepository offerings,
        ICourseRepository courses,
        ISemesterRepository semesters,
        IInstructorRepository instructors)
    {
        _offerings = offerings;
        _courses = courses;
        _semesters = semesters;
        _instructors = instructors;
    }

    public async Task<List<CourseOfferingResponse>> GetAllAsync(int? semesterId = null)
    {
        var offerings = await _offerings.GetAllAsync(semesterId);
        return offerings.Select(Map).ToList();
    }

    public async Task<CourseOfferingResponse> GetByIdAsync(int id)
    {
        var offering = await FindOfferingAsync(id);
        return Map(offering);
    }

    public async Task<CourseOfferingResponse> CreateAsync(CreateCourseOfferingRequest request)
    {
        var status = NormalizeStatus(request.Status);
        var course = await FindCourseForRequestAsync(request.CourseId);
        var semester = await FindSemesterForRequestAsync(request.SemesterId);
        var instructor = await FindInstructorForRequestAsync(request.InstructorId);

        if (await _offerings.OfferingKeyExistsAsync(course.CourseId, semester.SemesterId, instructor.InstructorId))
        {
            throw ApiException.Conflict("This course is already offered by the selected instructor in the selected semester.");
        }

        var offering = new CourseOffering
        {
            CourseId = course.CourseId,
            Course = course,
            SemesterId = semester.SemesterId,
            Semester = semester,
            InstructorId = instructor.InstructorId,
            Instructor = instructor,
            MaxCapacity = request.MaxCapacity,
            CurrentEnrollment = 0,
            IsGradeFinalized = false,
            Status = status
        };

        await _offerings.AddAsync(offering);
        await _offerings.SaveChangesAsync();
        return Map(offering);
    }

    public async Task<CourseOfferingResponse> UpdateAsync(int id, UpdateCourseOfferingRequest request)
    {
        var status = NormalizeStatus(request.Status);
        var offering = await FindOfferingAsync(id);
        var course = await FindCourseForRequestAsync(request.CourseId);
        var semester = await FindSemesterForRequestAsync(request.SemesterId);
        var instructor = await FindInstructorForRequestAsync(request.InstructorId);
        var activeEnrollmentCount = await _offerings.CountActiveEnrollmentsAsync(id);

        if (request.MaxCapacity < activeEnrollmentCount)
        {
            throw ApiException.Conflict("Max capacity cannot be lower than current active enrollment.");
        }

        if (await _offerings.OfferingKeyExistsAsync(course.CourseId, semester.SemesterId, instructor.InstructorId, id))
        {
            throw ApiException.Conflict("This course is already offered by the selected instructor in the selected semester.");
        }

        offering.CourseId = course.CourseId;
        offering.Course = course;
        offering.SemesterId = semester.SemesterId;
        offering.Semester = semester;
        offering.InstructorId = instructor.InstructorId;
        offering.Instructor = instructor;
        offering.MaxCapacity = request.MaxCapacity;
        offering.CurrentEnrollment = activeEnrollmentCount;
        offering.Status = status;

        await _offerings.SaveChangesAsync();
        return Map(offering);
    }

    public async Task DeleteAsync(int id)
    {
        var offering = await FindOfferingAsync(id);

        if (await _offerings.HasReferencesAsync(id))
        {
            throw ApiException.Conflict("Course offering cannot be deleted while enrollments, grade components, or attendance records reference it.");
        }

        _offerings.Remove(offering);
        await _offerings.SaveChangesAsync();
    }

    public static CourseOfferingResponse Map(CourseOffering offering)
    {
        var seatsAvailable = Math.Max(0, offering.MaxCapacity - offering.CurrentEnrollment);

        return new CourseOfferingResponse
        {
            OfferingId = offering.OfferingId,
            CourseId = offering.CourseId,
            CourseCode = offering.Course.CourseCode,
            CourseTitle = offering.Course.CourseTitle,
            CreditHours = offering.Course.CreditHours,
            DepartmentId = offering.Course.DepartmentId,
            DepartmentCode = offering.Course.Department.DepartmentCode,
            DepartmentName = offering.Course.Department.DepartmentName,
            SemesterId = offering.SemesterId,
            SemesterName = offering.Semester.SemesterName,
            IsCurrentSemester = offering.Semester.IsCurrent,
            InstructorId = offering.InstructorId,
            InstructorName = offering.Instructor.FullName,
            MaxCapacity = offering.MaxCapacity,
            CurrentEnrollment = offering.CurrentEnrollment,
            SeatsAvailable = seatsAvailable,
            IsFull = seatsAvailable <= 0,
            IsGradeFinalized = offering.IsGradeFinalized,
            Status = offering.Status
        };
    }

    private async Task<CourseOffering> FindOfferingAsync(int id)
    {
        return await _offerings.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Course offering was not found.");
    }

    private async Task<Course> FindCourseForRequestAsync(int id)
    {
        return await _courses.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Course was not found.");
    }

    private async Task<Semester> FindSemesterForRequestAsync(int id)
    {
        return await _semesters.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Semester was not found.");
    }

    private async Task<Instructor> FindInstructorForRequestAsync(int id)
    {
        var instructor = await _instructors.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Instructor was not found.");

        if (!instructor.User.IsActive)
        {
            throw ApiException.BadRequest("Inactive instructors cannot be assigned to course offerings.");
        }

        return instructor;
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToUpperInvariant();
        if (!ValidStatuses.Contains(normalized))
        {
            throw ApiException.BadRequest("Offering status must be ACTIVE, COMPLETED, or CANCELLED.");
        }

        return normalized;
    }
}
