using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly GpaSystemDbContext _db;

    public DepartmentRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Department>> GetAllAsync()
    {
        return _db.Departments
            .OrderBy(d => d.DepartmentCode)
            .ToListAsync();
    }

    public Task<Department?> GetByIdAsync(int id)
    {
        return _db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
    }

    public Task<bool> CodeExistsAsync(string departmentCode, int? excludeDepartmentId = null)
    {
        var normalized = departmentCode.Trim().ToUpper();
        return _db.Departments.AnyAsync(d =>
            d.DepartmentCode.ToUpper() == normalized &&
            (!excludeDepartmentId.HasValue || d.DepartmentId != excludeDepartmentId.Value));
    }

    public async Task<bool> HasReferencesAsync(int id)
    {
        return await _db.Students.AnyAsync(s => s.DepartmentId == id) ||
               await _db.Instructors.AnyAsync(i => i.DepartmentId == id) ||
               await _db.Courses.AnyAsync(c => c.DepartmentId == id);
    }

    public Task AddAsync(Department department)
    {
        return _db.Departments.AddAsync(department).AsTask();
    }

    public void Remove(Department department)
    {
        _db.Departments.Remove(department);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
