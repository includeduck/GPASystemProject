using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class GradeService : IGradeService
{
    private readonly GpaSystemDbContext _db;
    private readonly IGradeComponentRepository _components;
    private readonly IGradeEntryRepository _entries;
    private readonly ICourseOfferingRepository _offerings;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICourseGradeRepository _courseGrades;
    private readonly IGradingPolicyRepository _gradingPolicies;
    private readonly IGpaCalculatorService _gpaCalculator;
    private readonly IGradingStrategy _gradingStrategy;

    public GradeService(
        GpaSystemDbContext db,
        IGradeComponentRepository components,
        IGradeEntryRepository entries,
        ICourseOfferingRepository offerings,
        IEnrollmentRepository enrollments,
        ICourseGradeRepository courseGrades,
        IGradingPolicyRepository gradingPolicies,
        IGpaCalculatorService gpaCalculator,
        IGradingStrategy gradingStrategy)
    {
        _db = db;
        _components = components;
        _entries = entries;
        _offerings = offerings;
        _enrollments = enrollments;
        _courseGrades = courseGrades;
        _gradingPolicies = gradingPolicies;
        _gpaCalculator = gpaCalculator;
        _gradingStrategy = gradingStrategy;
    }

    public async Task<List<GradeComponentResponse>> GetComponentsAsync(int offeringId)
    {
        await FindOfferingAsync(offeringId);
        var list = await _components.GetByOfferingIdAsync(offeringId);
        return list.Select(Map).ToList();
    }

    public async Task<GradeComponentResponse> CreateComponentAsync(int offeringId, CreateGradeComponentRequest request)
    {
        var offering = await FindOfferingAsync(offeringId);
        if (offering.IsGradeFinalized)
        {
            throw ApiException.BadRequest("Cannot add grade components to a finalized course offering.");
        }

        if (string.IsNullOrWhiteSpace(request.ComponentName))
        {
            throw ApiException.BadRequest("Grade component name is required.");
        }

        if (request.MaxPoints <= 0)
        {
            throw ApiException.BadRequest("Maximum points must be greater than zero.");
        }

        if (await _components.ExistsByNameAsync(offeringId, request.ComponentName))
        {
            throw ApiException.Conflict($"A grade component named '{request.ComponentName}' already exists in this offering.");
        }

        var component = new GradeComponent
        {
            OfferingId = offeringId,
            ComponentName = request.ComponentName.Trim(),
            MaxPoints = request.MaxPoints,
            SortOrder = request.SortOrder
        };

        await _components.AddAsync(component);
        await _components.SaveChangesAsync();

        return Map(component);
    }

    public async Task<GradeComponentResponse> UpdateComponentAsync(int id, UpdateGradeComponentRequest request)
    {
        var component = await _components.GetByIdAsync(id)
            ?? throw ApiException.NotFound($"Grade component with ID {id} was not found.");

        if (component.CourseOffering.IsGradeFinalized)
        {
            throw ApiException.BadRequest("Cannot update grade components of a finalized course offering.");
        }

        if (string.IsNullOrWhiteSpace(request.ComponentName))
        {
            throw ApiException.BadRequest("Grade component name is required.");
        }

        if (request.MaxPoints <= 0)
        {
            throw ApiException.BadRequest("Maximum points must be greater than zero.");
        }

        if (await _components.ExistsByNameAsync(component.OfferingId, request.ComponentName, id))
        {
            throw ApiException.Conflict($"A grade component named '{request.ComponentName}' already exists in this offering.");
        }

        // Validate that no student has obtained marks exceeding the new MaxPoints
        var hasExceedingMarks = await _db.GradeEntries.AnyAsync(ge =>
            ge.ComponentId == id && ge.ObtainedMarks > request.MaxPoints);

        if (hasExceedingMarks)
        {
            throw ApiException.BadRequest($"Cannot set maximum points to {request.MaxPoints} because some student marks already exceed this value.");
        }

        component.ComponentName = request.ComponentName.Trim();
        component.MaxPoints = request.MaxPoints;
        component.SortOrder = request.SortOrder;

        await _components.SaveChangesAsync();

        return Map(component);
    }

    public async Task DeleteComponentAsync(int id)
    {
        var component = await _components.GetByIdAsync(id)
            ?? throw ApiException.NotFound($"Grade component with ID {id} was not found.");

        if (component.CourseOffering.IsGradeFinalized)
        {
            throw ApiException.BadRequest("Cannot delete grade components from a finalized course offering.");
        }

        // Prevent deletion if any student has marks recorded for this component
        var hasRecordedMarks = await _db.GradeEntries.AnyAsync(ge => ge.ComponentId == id);
        if (hasRecordedMarks)
        {
            throw ApiException.Conflict("Cannot delete this grade component because marks have already been recorded for students.");
        }

        _components.Remove(component);
        await _components.SaveChangesAsync();
    }

    public async Task<List<RosterGradeResponse>> GetGradebookRosterAsync(int offeringId)
    {
        var offering = await FindOfferingAsync(offeringId);
        var components = await _components.GetByOfferingIdAsync(offeringId);
        
        // Fetch all active enrollments for this offering
        var enrollments = await _db.Enrollments
            .Where(e => e.OfferingId == offeringId && (e.Status == "ENROLLED" || e.Status == "COMPLETED"))
            .Include(e => e.Student)
            .Include(e => e.CourseGrade)
            .OrderBy(e => e.Student.StudentNumber)
            .ToListAsync();

        var allEntries = await _entries.GetByOfferingIdAsync(offeringId);

        var roster = new List<RosterGradeResponse>();

        foreach (var enrollment in enrollments)
        {
            var studentEntries = allEntries.Where(ge => ge.EnrollmentId == enrollment.EnrollmentId).ToList();

            var mappedEntries = studentEntries.Select(ge => new GradeEntryResponse
            {
                GradeEntryId = ge.GradeEntryId,
                EnrollmentId = ge.EnrollmentId,
                ComponentId = ge.ComponentId,
                ObtainedMarks = ge.ObtainedMarks,
                RecordedBy = ge.RecordedBy,
                InstructorName = ge.Instructor?.FullName ?? "System",
                RecordedAt = ge.RecordedAt,
                LastModifiedAt = ge.LastModifiedAt
            }).ToList();

            var finalGrade = enrollment.CourseGrade;

            roster.Add(new RosterGradeResponse
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                StudentNumber = enrollment.Student.StudentNumber,
                StudentName = enrollment.Student.FullName,
                Entries = mappedEntries,
                TotalObtained = finalGrade?.TotalObtained,
                MaxPossible = finalGrade?.MaxPossible,
                Percentage = finalGrade?.Percentage,
                LetterGrade = finalGrade?.LetterGrade,
                GradePoints = finalGrade?.GradePoints,
                EnrollmentStatus = enrollment.Status
            });
        }

        return roster;
    }

    public async Task RecordMarksAsync(int offeringId, List<RecordGradeEntryRequest> requests, int instructorId)
    {
        var offering = await FindOfferingAsync(offeringId);
        await EnsureInstructorExistsAsync(instructorId);
        if (offering.IsGradeFinalized)
        {
            throw ApiException.BadRequest("Cannot modify grades. The course offering is already finalized.");
        }

        var components = await _components.GetByOfferingIdAsync(offeringId);
        var componentMap = components.ToDictionary(c => c.ComponentId);

        foreach (var req in requests)
        {
            if (!componentMap.TryGetValue(req.ComponentId, out var component))
            {
                throw ApiException.BadRequest($"Component with ID {req.ComponentId} does not belong to this course offering.");
            }

            if (req.ObtainedMarks < 0 || req.ObtainedMarks > component.MaxPoints)
            {
                throw ApiException.BadRequest($"Obtained mark ({req.ObtainedMarks}) for component '{component.ComponentName}' is out of bounds [0, {component.MaxPoints}].");
            }

            var enrollment = await _db.Enrollments.FindAsync(req.EnrollmentId);
            if (enrollment == null || enrollment.OfferingId != offeringId)
            {
                throw ApiException.BadRequest($"Enrollment with ID {req.EnrollmentId} is invalid for this offering.");
            }

            var entry = await _entries.GetSingleEntryAsync(req.EnrollmentId, req.ComponentId);
            if (entry == null)
            {
                entry = new GradeEntry
                {
                    EnrollmentId = req.EnrollmentId,
                    ComponentId = req.ComponentId,
                    ObtainedMarks = req.ObtainedMarks,
                    RecordedBy = instructorId,
                    RecordedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };
                await _entries.AddAsync(entry);
            }
            else
            {
                entry.ObtainedMarks = req.ObtainedMarks;
                entry.RecordedBy = instructorId;
                entry.LastModifiedAt = DateTime.UtcNow;
                _db.Entry(entry).State = EntityState.Modified;
            }
        }

        await _entries.SaveChangesAsync();
    }

    public async Task FinalizeGradesAsync(int offeringId, bool force, int instructorId)
    {
        var offering = await FindOfferingAsync(offeringId);
        var actorUserId = await EnsureInstructorExistsAsync(instructorId);
        if (offering.IsGradeFinalized)
        {
            throw ApiException.BadRequest("Grades for this offering have already been finalized.");
        }

        var components = await _components.GetByOfferingIdAsync(offeringId);
        if (components.Count == 0)
        {
            throw ApiException.BadRequest("Cannot finalize grades because no grade components have been defined for this course offering.");
        }

        var enrollments = await _db.Enrollments
            .Where(e => e.OfferingId == offeringId && e.Status == "ENROLLED")
            .Include(e => e.Student)
            .ToListAsync();

        if (enrollments.Count == 0)
        {
            // Lock and complete offering directly if no students are active
            offering.IsGradeFinalized = true;
            offering.Status = "COMPLETED";
            await _offerings.SaveChangesAsync();
            return;
        }

        var allEntries = await _entries.GetByOfferingIdAsync(offeringId);
        var entriesByEnrollment = allEntries.GroupBy(ge => ge.EnrollmentId).ToDictionary(g => g.Key, g => g.ToList());

        var missingMarksExist = false;
        var missingGradeEntriesToAdd = new List<GradeEntry>();

        foreach (var enrollment in enrollments)
        {
            var studentEntries = entriesByEnrollment.TryGetValue(enrollment.EnrollmentId, out var list) ? list : new List<GradeEntry>();
            var recordedComponentIds = studentEntries.Select(se => se.ComponentId).ToHashSet();

            foreach (var comp in components)
            {
                if (!recordedComponentIds.Contains(comp.ComponentId))
                {
                    missingMarksExist = true;
                    // Prepare automatic 0 entry
                    missingGradeEntriesToAdd.Add(new GradeEntry
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        ComponentId = comp.ComponentId,
                        ObtainedMarks = 0m,
                        RecordedBy = instructorId,
                        RecordedAt = DateTime.UtcNow,
                        LastModifiedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (missingMarksExist && !force)
        {
            throw ApiException.BadRequest("Some students have missing marks. Please fill in all marks, or confirm finalization to treat missing marks as zero.");
        }

        // Load active policies and configurations
        var activePolicies = await _gradingPolicies.GetActivePoliciesAsync();
        var passFailCutoffStr = await _db.Configurations
            .Where(c => c.ConfigKey == "pass_fail_cutoff")
            .Select(c => c.ConfigValue)
            .FirstOrDefaultAsync() ?? "50";

        if (!decimal.TryParse(passFailCutoffStr, out var passFailCutoff))
        {
            passFailCutoff = 50m;
        }

        var maxPossibleTotal = components.Sum(c => c.MaxPoints);
        if (maxPossibleTotal <= 0)
        {
            throw ApiException.BadRequest("Total maximum possible points across components must be greater than zero.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. Insert missing marks as 0 if forcing finalization
            if (missingGradeEntriesToAdd.Count > 0)
            {
                foreach (var missingEntry in missingGradeEntriesToAdd)
                {
                    await _entries.AddAsync(missingEntry);
                }
                await _entries.SaveChangesAsync();
                
                // Re-fetch all entries now that zero entries are saved
                allEntries = await _entries.GetByOfferingIdAsync(offeringId);
                entriesByEnrollment = allEntries.GroupBy(ge => ge.EnrollmentId).ToDictionary(g => g.Key, g => g.ToList());
            }

            // 2. Generate CourseGrades and mark enrollments COMPLETED
            foreach (var enrollment in enrollments)
            {
                var studentEntries = entriesByEnrollment.TryGetValue(enrollment.EnrollmentId, out var list) ? list : new List<GradeEntry>();
                var obtainedTotal = studentEntries.Sum(ge => ge.ObtainedMarks);
                var percentage = Math.Round((obtainedTotal / maxPossibleTotal) * 100m, 2, MidpointRounding.AwayFromZero);

                var (letterGrade, gradePoint) = _gradingStrategy.MapPercentage(percentage, activePolicies, passFailCutoff);

                var courseGrade = await _courseGrades.GetByEnrollmentIdAsync(enrollment.EnrollmentId);
                if (courseGrade == null)
                {
                    courseGrade = new CourseGrade
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        TotalObtained = obtainedTotal,
                        MaxPossible = maxPossibleTotal,
                        Percentage = percentage,
                        LetterGrade = letterGrade,
                        GradePoints = gradePoint,
                        IsRepeatedAttempt = false,
                        CalculatedAt = DateTime.UtcNow
                    };
                    await _courseGrades.AddAsync(courseGrade);
                }
                else
                {
                    courseGrade.TotalObtained = obtainedTotal;
                    courseGrade.MaxPossible = maxPossibleTotal;
                    courseGrade.Percentage = percentage;
                    courseGrade.LetterGrade = letterGrade;
                    courseGrade.GradePoints = gradePoint;
                    courseGrade.CalculatedAt = DateTime.UtcNow;
                    _db.Entry(courseGrade).State = EntityState.Modified;
                }

                enrollment.Status = "COMPLETED";
                _db.Entry(enrollment).State = EntityState.Modified;
            }

            // 3. Mark CourseOffering finalized
            offering.IsGradeFinalized = true;
            offering.Status = "COMPLETED";
            _db.Entry(offering).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // 4. Recalculate GPA/CGPA for all affected students sequentially
            var uniqueStudentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();
            foreach (var sId in uniqueStudentIds)
            {
                await _gpaCalculator.RecalculateStudentGpaAndCgpaAsync(sId);
            }

            // 5. Add notifications to database audit log & user alert
            var courseCode = offering.Course?.CourseCode ?? "Course";
            var semesterName = offering.Semester?.SemesterName ?? "Semester";
            foreach (var enrollment in enrollments)
            {
                var notification = new Notification
                {
                    UserId = enrollment.Student.UserId,
                    Type = "IN_APP",
                    Subject = "Course Results Published",
                    MessageBody = $"Dear {enrollment.Student.FullName}, your results for {courseCode} in {semesterName} are now published. Log in to view your transcript.",
                    SentStatus = "SENT",
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _db.Notification.AddAsync(notification);
            }

            // AuditLog grade finalization
            var auditLog = new AuditLog
            {
                UserId = actorUserId,
                ActionType = "GRADE_FINALIZATION",
                TableName = "CourseOffering",
                RecordId = offeringId,
                OldValue = "is_grade_finalized: 0",
                NewValue = "is_grade_finalized: 1",
                LoggedAt = DateTime.UtcNow
            };
            await _db.AuditLog.AddAsync(auditLog);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<CourseOffering> FindOfferingAsync(int id)
    {
        return await _offerings.GetByIdAsync(id)
            ?? throw ApiException.NotFound($"Course offering with ID {id} was not found.");
    }

    private async Task<int> EnsureInstructorExistsAsync(int instructorId)
    {
        var userId = await _db.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.UserId)
            .FirstOrDefaultAsync();

        if (userId == 0)
        {
            throw ApiException.BadRequest("Instructor was not found.");
        }

        return userId;
    }

    private static GradeComponentResponse Map(GradeComponent component)
    {
        return new GradeComponentResponse
        {
            ComponentId = component.ComponentId,
            OfferingId = component.OfferingId,
            ComponentName = component.ComponentName,
            MaxPoints = component.MaxPoints,
            SortOrder = component.SortOrder
        };
    }
}
