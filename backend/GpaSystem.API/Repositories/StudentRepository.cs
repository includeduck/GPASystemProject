using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly GpaSystemDbContext _db;

    public StudentRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Student>> GetAllAsync()
    {
        return _db.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .OrderBy(s => s.StudentNumber)
            .ToListAsync();
    }

    public Task<Student?> GetByIdAsync(int id)
    {
        return _db.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        var normalized = email.Trim().ToUpper();
        return _db.AppUsers.AnyAsync(u =>
            u.Email.ToUpper() == normalized &&
            (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public Task<int> CountByStudentNumberPrefixAsync(string prefix)
    {
        return _db.Students.CountAsync(s => s.StudentNumber.StartsWith(prefix));
    }

    public Task<bool> StudentNumberExistsAsync(string studentNumber)
    {
        return _db.Students.AnyAsync(s => s.StudentNumber == studentNumber);
    }

    public Task AddAsync(Student student)
    {
        return _db.Students.AddAsync(student).AsTask();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
