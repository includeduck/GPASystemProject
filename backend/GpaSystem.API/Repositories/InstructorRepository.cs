using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly GpaSystemDbContext _db;

    public InstructorRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Instructor>> GetAllAsync()
    {
        return _db.Instructors
            .Include(i => i.User)
            .Include(i => i.Department)
            .OrderBy(i => i.FullName)
            .ToListAsync();
    }

    public Task<Instructor?> GetByIdAsync(int id)
    {
        return _db.Instructors
            .Include(i => i.User)
            .Include(i => i.Department)
            .FirstOrDefaultAsync(i => i.InstructorId == id);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        var normalized = email.Trim().ToUpper();
        return _db.AppUsers.AnyAsync(u =>
            u.Email.ToUpper() == normalized &&
            (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public Task AddAsync(Instructor instructor)
    {
        return _db.Instructors.AddAsync(instructor).AsTask();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
