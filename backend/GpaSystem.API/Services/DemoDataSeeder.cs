using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class DemoDataSeeder
{
    private readonly GpaSystemDbContext _db;
    private readonly IGradingPolicyService _gradingPolicyService;
    private readonly ISemesterService _semesterService;
    private readonly ICourseOfferingService _courseOfferingService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGradeService _gradeService;

    public DemoDataSeeder(
        GpaSystemDbContext db,
        IGradingPolicyService gradingPolicyService,
        ISemesterService semesterService,
        ICourseOfferingService courseOfferingService,
        IEnrollmentService enrollmentService,
        IGradeService gradeService)
    {
        _db = db;
        _gradingPolicyService = gradingPolicyService;
        _semesterService = semesterService;
        _courseOfferingService = courseOfferingService;
        _enrollmentService = enrollmentService;
        _gradeService = gradeService;
    }

    public async Task<DemoSeedResult> SeedAsync()
    {
        await EnsureGradingPoliciesAsync();
        await _gradingPolicyService.UpdatePassingCutoffAsync(50m);

        var semester = await EnsureCurrentSemesterAsync();
        var course = await _db.Courses.OrderBy(c => c.CourseId).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Seed requires at least one course. Create a course first.");
        var instructor = await _db.Instructors
            .Include(i => i.User)
            .Where(i => i.User.IsActive)
            .OrderBy(i => i.InstructorId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Seed requires at least one active instructor.");
        var student = await _db.Students
            .Include(s => s.User)
            .Where(s => s.Status == "ACTIVE" && s.User.IsActive)
            .OrderBy(s => s.StudentId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Seed requires at least one active student.");

        var offering = await EnsureDemoOfferingAsync(course.CourseId, semester.SemesterId, instructor.InstructorId);
        var enrollmentId = await EnsureEnrollmentAsync(student.StudentId, offering.OfferingId);
        await EnsureGradeComponentsAsync(offering.OfferingId, enrollmentId, instructor.InstructorId);

        return new DemoSeedResult
        {
            SemesterId = semester.SemesterId,
            CourseId = course.CourseId,
            OfferingId = offering.OfferingId,
            StudentId = student.StudentId,
            EnrollmentId = enrollmentId,
            InstructorId = instructor.InstructorId
        };
    }

    private async Task EnsureGradingPoliciesAsync()
    {
        if (await _db.GradingPolicies.AnyAsync())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        await _gradingPolicyService.UpdatePoliciesAsync(
        [
            new UpdateGradingPolicyRequest { LetterGrade = "A", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.00m, IsActive = true, EffectiveFrom = today },
            new UpdateGradingPolicyRequest { LetterGrade = "B", MinPercentage = 80, MaxPercentage = 90, GradePoint = 3.00m, IsActive = true, EffectiveFrom = today },
            new UpdateGradingPolicyRequest { LetterGrade = "C", MinPercentage = 70, MaxPercentage = 80, GradePoint = 2.00m, IsActive = true, EffectiveFrom = today },
            new UpdateGradingPolicyRequest { LetterGrade = "D", MinPercentage = 60, MaxPercentage = 70, GradePoint = 1.00m, IsActive = true, EffectiveFrom = today },
            new UpdateGradingPolicyRequest { LetterGrade = "F", MinPercentage = 0, MaxPercentage = 60, GradePoint = 0m, IsActive = true, EffectiveFrom = today }
        ]);
    }

    private async Task<SemesterResponse> EnsureCurrentSemesterAsync()
    {
        var current = await _db.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
        if (current != null)
        {
            return new SemesterResponse
            {
                SemesterId = current.SemesterId,
                SemesterName = current.SemesterName,
                StartDate = current.StartDate,
                EndDate = current.EndDate,
                IsCurrent = current.IsCurrent
            };
        }

        var existing = await _db.Semesters.OrderByDescending(s => s.SemesterId).FirstOrDefaultAsync();
        if (existing != null)
        {
            return await _semesterService.SetCurrentAsync(existing.SemesterId);
        }

        return await _semesterService.CreateAsync(new CreateSemesterRequest
        {
            SemesterName = "Spring 2026",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 5, 30),
            IsCurrent = true
        });
    }

    private async Task<CourseOfferingResponse> EnsureDemoOfferingAsync(int courseId, int semesterId, int instructorId)
    {
        var existing = await _db.CourseOfferings
            .FirstOrDefaultAsync(o =>
                o.CourseId == courseId &&
                o.SemesterId == semesterId &&
                o.InstructorId == instructorId &&
                !o.IsGradeFinalized);

        if (existing != null)
        {
            return await _courseOfferingService.GetByIdAsync(existing.OfferingId);
        }

        return await _courseOfferingService.CreateAsync(new CreateCourseOfferingRequest
        {
            CourseId = courseId,
            SemesterId = semesterId,
            InstructorId = instructorId,
            MaxCapacity = 30,
            Status = "ACTIVE"
        });
    }

    private async Task<int> EnsureEnrollmentAsync(int studentId, int offeringId)
    {
        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.OfferingId == offeringId);

        if (existing != null)
        {
            return existing.EnrollmentId;
        }

        var created = await _enrollmentService.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = studentId,
            OfferingId = offeringId
        });
        return created.EnrollmentId;
    }

    private async Task EnsureGradeComponentsAsync(int offeringId, int enrollmentId, int instructorId)
    {
        if (await _db.GradeEntries.AnyAsync(e => e.EnrollmentId == enrollmentId))
        {
            return;
        }

        var componentIds = await _db.GradeComponents
            .Where(c => c.OfferingId == offeringId)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.ComponentId)
            .ToListAsync();

        if (componentIds.Count == 0)
        {
            var midterm = await _gradeService.CreateComponentAsync(offeringId, new CreateGradeComponentRequest
            {
                ComponentName = "Midterm",
                MaxPoints = 40,
                SortOrder = 1
            });
            var finalExam = await _gradeService.CreateComponentAsync(offeringId, new CreateGradeComponentRequest
            {
                ComponentName = "Final Exam",
                MaxPoints = 60,
                SortOrder = 2
            });
            componentIds = [midterm.ComponentId, finalExam.ComponentId];
        }

        var midtermId = componentIds[0];
        var finalId = componentIds.Count > 1 ? componentIds[1] : componentIds[0];

        await _gradeService.RecordMarksAsync(
            offeringId,
            new List<RecordGradeEntryRequest>
            {
                new() { EnrollmentId = enrollmentId, ComponentId = midtermId, ObtainedMarks = 34 },
                new() { EnrollmentId = enrollmentId, ComponentId = finalId, ObtainedMarks = 52 }
            },
            instructorId);
    }

}

public class DemoSeedResult
{
    public int SemesterId { get; set; }
    public int CourseId { get; set; }
    public int OfferingId { get; set; }
    public int StudentId { get; set; }
    public int EnrollmentId { get; set; }
    public int InstructorId { get; set; }
}
