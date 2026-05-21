using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class CourseGradeRepository : ICourseGradeRepository
{
    private readonly GpaSystemDbContext _db;

    public CourseGradeRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<CourseGrade?> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return _db.CourseGrades
            .FirstOrDefaultAsync(cg => cg.EnrollmentId == enrollmentId);
    }

    public Task<List<CourseGrade>> GetForStudentAsync(int studentId)
    {
        return _db.CourseGrades
            .Where(cg => cg.Enrollment.StudentId == studentId && cg.Enrollment.Status == "COMPLETED")
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
            .OrderBy(cg => cg.Enrollment.CourseOffering.Semester.StartDate)
            .ToListAsync();
    }

    public Task AddAsync(CourseGrade grade)
    {
        return _db.CourseGrades.AddAsync(grade).AsTask();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
