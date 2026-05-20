using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly GpaSystemDbContext _db;

    public EnrollmentRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Enrollment>> GetByStudentAsync(int studentId)
    {
        return IncludeDetails(_db.Enrollments.AsQueryable())
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ThenBy(e => e.CourseOffering.Course.CourseCode)
            .ToListAsync();
    }

    public Task<Enrollment?> GetByIdAsync(int id)
    {
        return IncludeDetails(_db.Enrollments.AsQueryable())
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public Task<bool> ExistsAsync(int studentId, int offeringId)
    {
        return _db.Enrollments.AnyAsync(e => e.StudentId == studentId && e.OfferingId == offeringId);
    }

    public Task AddAsync(Enrollment enrollment)
    {
        return _db.Enrollments.AddAsync(enrollment).AsTask();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }

    private static IQueryable<Enrollment> IncludeDetails(IQueryable<Enrollment> query)
    {
        return query
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Semester)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Instructor);
    }
}
