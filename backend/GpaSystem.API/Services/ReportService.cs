using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class ReportService : IReportService
{
    private const decimal DefaultWarningThreshold = 2.0m;
    private const string WarningThresholdKey = "warning_gpa_threshold";

    private readonly GpaSystemDbContext _db;
    private readonly IStudentRepository _students;
    private readonly IAcademicRecordRepository _academicRecords;
    private readonly ISemesterRepository _semesters;
    private readonly IDepartmentRepository _departments;
    private readonly ICourseRepository _courses;

    public ReportService(
        GpaSystemDbContext db,
        IStudentRepository students,
        IAcademicRecordRepository academicRecords,
        ISemesterRepository semesters,
        IDepartmentRepository departments,
        ICourseRepository courses)
    {
        _db = db;
        _students = students;
        _academicRecords = academicRecords;
        _semesters = semesters;
        _departments = departments;
        _courses = courses;
    }

    public Task<StudentDashboardResponse> GetStudentDashboardAsync(int studentId) =>
        BuildStudentDashboardAsync(studentId);

    public async Task<TranscriptResponse> GetTranscriptAsync(int studentId)
    {
        var student = await _students.GetByIdAsync(studentId)
            ?? throw ApiException.NotFound($"Student with ID {studentId} was not found.");

        var dashboard = await BuildStudentDashboardAsync(studentId);
        var failedCourses = dashboard.Semesters
            .SelectMany(s => s.Courses)
            .Where(c => c.Status == "FAILED" && !c.IsRepeatedAttempt)
            .ToList();

        return new TranscriptResponse
        {
            StudentId = dashboard.StudentId,
            StudentNumber = dashboard.StudentNumber,
            FullName = dashboard.FullName,
            DepartmentCode = student.Department.DepartmentCode,
            DepartmentName = student.Department.DepartmentName,
            EnrollmentDate = student.EnrollmentDate,
            CGPA = dashboard.CGPA,
            TotalCreditsAttempted = dashboard.TotalCreditsAttempted,
            TotalCreditsEarned = dashboard.TotalCreditsEarned,
            GeneratedAt = DateTime.UtcNow,
            Semesters = dashboard.Semesters,
            FailedCourses = failedCourses
        };
    }

    public async Task<SemesterResultsReportResponse> GetSemesterResultsAsync(int semesterId)
    {
        var semester = await _semesters.GetByIdAsync(semesterId)
            ?? throw ApiException.NotFound("Semester was not found.");

        var records = await _academicRecords.GetForSemesterAsync(semesterId);
        var studentResults = new List<SemesterStudentResultResponse>();

        foreach (var record in records.OrderBy(r => r.Student.StudentNumber))
        {
            var courses = await GetCompletedCourseGradesAsync(record.StudentId, semesterId);
            studentResults.Add(new SemesterStudentResultResponse
            {
                StudentId = record.StudentId,
                StudentNumber = record.Student.StudentNumber,
                FullName = record.Student.FullName,
                DepartmentCode = record.Student.Department.DepartmentCode,
                SemesterGpa = record.SemesterGpa,
                CumulativeGpa = record.CumulativeGpa,
                CreditsAttempted = record.TotalCreditsAttempted,
                Courses = courses
            });
        }

        return new SemesterResultsReportResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            Students = studentResults
        };
    }

    public async Task<CoursePerformanceReportResponse> GetCoursePerformanceAsync(int courseId, int? semesterId = null, int? instructorId = null)
    {
        var course = await _courses.GetByIdAsync(courseId)
            ?? throw ApiException.NotFound("Course was not found.");

        var offeringsQuery = _db.CourseOfferings
            .Where(o => o.CourseId == courseId && o.IsGradeFinalized)
            .Include(o => o.Semester)
            .Include(o => o.Instructor)
            .AsNoTracking()
            .AsQueryable();

        if (semesterId.HasValue)
        {
            offeringsQuery = offeringsQuery.Where(o => o.SemesterId == semesterId.Value);
        }

        if (instructorId.HasValue)
        {
            offeringsQuery = offeringsQuery.Where(o => o.InstructorId == instructorId.Value);
        }

        var offerings = await offeringsQuery.ToListAsync();
        var offeringPerformances = new List<CourseOfferingPerformanceResponse>();
        var allPercentages = new List<decimal>();

        foreach (var offering in offerings)
        {
            var grades = await _db.CourseGrades
                .Where(cg => cg.Enrollment.OfferingId == offering.OfferingId && !cg.IsRepeatedAttempt)
                .Select(cg => cg.Percentage)
                .ToListAsync();

            if (grades.Count == 0)
            {
                continue;
            }

            allPercentages.AddRange(grades);
            offeringPerformances.Add(new CourseOfferingPerformanceResponse
            {
                OfferingId = offering.OfferingId,
                SemesterName = offering.Semester.SemesterName,
                InstructorName = offering.Instructor.FullName,
                EnrollmentCount = grades.Count,
                AveragePercentage = Math.Round(grades.Average(), 2, MidpointRounding.AwayFromZero)
            });
        }

        var passed = allPercentages.Count > 0
            ? await CountPassingGradesForCourseAsync(courseId, semesterId, instructorId)
            : 0;
        var total = allPercentages.Count;

        return new CoursePerformanceReportResponse
        {
            CourseId = course.CourseId,
            CourseCode = course.CourseCode,
            CourseTitle = course.CourseTitle,
            SemesterId = semesterId,
            SemesterName = semesterId.HasValue
                ? (await _semesters.GetByIdAsync(semesterId.Value))?.SemesterName
                : null,
            TotalEnrollments = total,
            PassedCount = passed,
            FailedCount = total - passed,
            AveragePercentage = total > 0
                ? Math.Round(allPercentages.Average(), 2, MidpointRounding.AwayFromZero)
                : 0m,
            Offerings = offeringPerformances
        };
    }

    public async Task<DepartmentPerformanceReportResponse> GetDepartmentPerformanceAsync(int departmentId, int? semesterId = null)
    {
        var department = await _departments.GetByIdAsync(departmentId)
            ?? throw ApiException.NotFound("Department was not found.");

        var students = await _db.Students
            .Where(s => s.DepartmentId == departmentId && s.Status == "ACTIVE")
            .Include(s => s.Department)
            .ToListAsync();

        var summaries = new List<DepartmentStudentSummaryResponse>();
        var gpas = new List<decimal>();
        var passRates = new List<decimal>();

        foreach (var student in students)
        {
            var record = semesterId.HasValue
                ? await _academicRecords.GetByStudentAndSemesterAsync(student.StudentId, semesterId.Value)
                : (await _academicRecords.GetForStudentAsync(student.StudentId))
                    .OrderByDescending(r => r.Semester.StartDate)
                    .FirstOrDefault();

            if (record == null)
            {
                continue;
            }

            gpas.Add(record.SemesterGpa);
            summaries.Add(new DepartmentStudentSummaryResponse
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                SemesterGpa = record.SemesterGpa,
                CumulativeGpa = record.CumulativeGpa
            });

            if (semesterId.HasValue)
            {
                var courses = await GetCompletedCourseGradesAsync(student.StudentId, semesterId.Value);
                var active = courses.Where(c => !c.IsRepeatedAttempt).ToList();
                if (active.Count > 0)
                {
                    passRates.Add((decimal)active.Count(c => c.GradePoints > 0) / active.Count);
                }
            }
        }

        return new DepartmentPerformanceReportResponse
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            SemesterId = semesterId,
            SemesterName = semesterId.HasValue
                ? (await _semesters.GetByIdAsync(semesterId.Value))?.SemesterName
                : null,
            StudentCount = summaries.Count,
            AverageSemesterGpa = gpas.Count > 0
                ? Math.Round(gpas.Average(), 2, MidpointRounding.AwayFromZero)
                : 0m,
            PassRate = passRates.Count > 0
                ? Math.Round(passRates.Average() * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m,
            Students = summaries.OrderByDescending(s => s.SemesterGpa).ToList()
        };
    }

    public async Task<WarningListReportResponse> GetWarningListAsync(int semesterId, decimal? threshold = null)
    {
        var semester = await _semesters.GetByIdAsync(semesterId)
            ?? throw ApiException.NotFound("Semester was not found.");

        var cutoff = threshold ?? await GetWarningGpaThresholdAsync();
        var records = await _academicRecords.GetForSemesterAsync(semesterId);

        var warnings = records
            .Where(r => r.SemesterGpa < cutoff)
            .OrderBy(r => r.SemesterGpa)
            .Select(r => new WarningStudentResponse
            {
                StudentId = r.StudentId,
                StudentNumber = r.Student.StudentNumber,
                FullName = r.Student.FullName,
                DepartmentCode = r.Student.Department.DepartmentCode,
                SemesterGpa = r.SemesterGpa,
                CumulativeGpa = r.CumulativeGpa
            })
            .ToList();

        return new WarningListReportResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            Threshold = cutoff,
            Students = warnings
        };
    }

    public async Task<ClassRankingsReportResponse> GetClassRankingsAsync(int? departmentId = null, int? semesterId = null)
    {
        var studentsQuery = _db.Students
            .Include(s => s.Department)
            .Where(s => s.Status == "ACTIVE")
            .AsQueryable();

        if (departmentId.HasValue)
        {
            studentsQuery = studentsQuery.Where(s => s.DepartmentId == departmentId.Value);
        }

        var students = await studentsQuery.ToListAsync();
        var entries = new List<(Student Student, decimal Cgpa)>();

        foreach (var student in students)
        {
            decimal cgpa;
            if (semesterId.HasValue)
            {
                var record = await _academicRecords.GetByStudentAndSemesterAsync(student.StudentId, semesterId.Value);
                if (record == null)
                {
                    continue;
                }

                cgpa = record.CumulativeGpa;
            }
            else
            {
                var records = await _academicRecords.GetForStudentAsync(student.StudentId);
                var latest = records.OrderByDescending(r => r.Semester.StartDate).FirstOrDefault();
                if (latest == null)
                {
                    continue;
                }

                cgpa = latest.CumulativeGpa;
            }

            entries.Add((student, cgpa));
        }

        var ranked = entries
            .OrderByDescending(e => e.Cgpa)
            .ThenBy(e => e.Student.StudentId)
            .Select((e, index) => new ClassRankingEntryResponse
            {
                Rank = index + 1,
                StudentId = e.Student.StudentId,
                StudentNumber = e.Student.StudentNumber,
                FullName = e.Student.FullName,
                DepartmentCode = e.Student.Department.DepartmentCode,
                Cgpa = e.Cgpa
            })
            .ToList();

        Department? dept = null;
        if (departmentId.HasValue)
        {
            dept = await _departments.GetByIdAsync(departmentId.Value);
        }

        Semester? semester = null;
        if (semesterId.HasValue)
        {
            semester = await _semesters.GetByIdAsync(semesterId.Value);
        }

        return new ClassRankingsReportResponse
        {
            SemesterId = semesterId,
            SemesterName = semester?.SemesterName,
            DepartmentId = departmentId,
            DepartmentCode = dept?.DepartmentCode,
            Rankings = ranked
        };
    }

    public async Task<decimal> GetWarningGpaThresholdAsync()
    {
        var config = await _db.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == WarningThresholdKey);
        if (config != null && decimal.TryParse(config.ConfigValue, out var threshold))
        {
            return threshold;
        }

        return DefaultWarningThreshold;
    }

    private async Task<StudentDashboardResponse> BuildStudentDashboardAsync(int studentId)
    {
        var student = await _students.GetByIdAsync(studentId)
            ?? throw ApiException.NotFound($"Student with ID {studentId} was not found.");

        var records = await _academicRecords.GetForStudentAsync(studentId);
        var allGrades = await _db.CourseGrades
            .Where(cg => cg.Enrollment.StudentId == studentId && cg.Enrollment.Status == "COMPLETED")
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
            .ToListAsync();

        var latestRecord = records.OrderByDescending(r => r.Semester.StartDate).FirstOrDefault();
        var cgpa = latestRecord?.CumulativeGpa ?? 0.00m;

        var activeGrades = allGrades.Where(g => !g.IsRepeatedAttempt).ToList();
        var totalAttempted = activeGrades.Sum(g => (int)g.Enrollment.CourseOffering.Course.CreditHours);
        var totalEarned = activeGrades
            .Where(g => g.GradePoints > 0m)
            .Sum(g => (int)g.Enrollment.CourseOffering.Course.CreditHours);

        var semesterResponses = new List<SemesterResultResponse>();

        foreach (var record in records.OrderBy(r => r.Semester.StartDate))
        {
            var semGrades = allGrades
                .Where(g => g.Enrollment.CourseOffering.SemesterId == record.SemesterId)
                .ToList();

            var semActiveGrades = semGrades.Where(g => !g.IsRepeatedAttempt).ToList();
            var semCreditsEarned = semActiveGrades
                .Where(g => g.GradePoints > 0m)
                .Sum(g => (int)g.Enrollment.CourseOffering.Course.CreditHours);

            var courseResponses = semGrades.Select(MapCourseGrade).ToList();

            semesterResponses.Add(new SemesterResultResponse
            {
                SemesterId = record.SemesterId,
                SemesterName = record.Semester.SemesterName,
                GPA = record.SemesterGpa,
                CGPA = record.CumulativeGpa,
                CreditsAttempted = record.TotalCreditsAttempted,
                CreditsEarned = semCreditsEarned,
                Courses = courseResponses
            });
        }

        return new StudentDashboardResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            StudentNumber = student.StudentNumber,
            CGPA = cgpa,
            TotalCreditsAttempted = totalAttempted,
            TotalCreditsEarned = totalEarned,
            Semesters = semesterResponses
        };
    }

    private async Task<List<StudentCourseGradeResponse>> GetCompletedCourseGradesAsync(int studentId, int semesterId)
    {
        var grades = await _db.CourseGrades
            .Where(cg =>
                cg.Enrollment.StudentId == studentId &&
                cg.Enrollment.Status == "COMPLETED" &&
                cg.Enrollment.CourseOffering.SemesterId == semesterId)
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
            .ToListAsync();

        return grades.Select(MapCourseGrade).ToList();
    }

    private async Task<int> CountPassingGradesForCourseAsync(int courseId, int? semesterId, int? instructorId)
    {
        var query = _db.CourseGrades
            .Where(cg =>
                cg.Enrollment.CourseOffering.CourseId == courseId &&
                !cg.IsRepeatedAttempt &&
                cg.GradePoints > 0);

        if (semesterId.HasValue)
        {
            query = query.Where(cg => cg.Enrollment.CourseOffering.SemesterId == semesterId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(cg => cg.Enrollment.CourseOffering.InstructorId == instructorId.Value);
        }

        return await query.CountAsync();
    }

    private static StudentCourseGradeResponse MapCourseGrade(CourseGrade g) =>
        new()
        {
            CourseId = g.Enrollment.CourseOffering.CourseId,
            CourseCode = g.Enrollment.CourseOffering.Course.CourseCode,
            CourseTitle = g.Enrollment.CourseOffering.Course.CourseTitle,
            CreditHours = g.Enrollment.CourseOffering.Course.CreditHours,
            TotalObtained = g.TotalObtained,
            MaxPossible = g.MaxPossible,
            Percentage = g.Percentage,
            LetterGrade = g.LetterGrade,
            GradePoints = g.GradePoints,
            IsRepeatedAttempt = g.IsRepeatedAttempt,
            Status = g.GradePoints > 0m ? "PASSED" : "FAILED"
        };
}
