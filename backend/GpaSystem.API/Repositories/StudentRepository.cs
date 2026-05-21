using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
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

    public async Task<(List<StudentListItemResponse> Items, int TotalCount)> SearchAsync(StudentSearchQuery query)
    {
        var studentsQuery = _db.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            studentsQuery = studentsQuery.Where(s =>
                s.FullName.ToLower().Contains(term) ||
                s.StudentNumber.ToLower().Contains(term));
        }

        if (query.DepartmentId.HasValue)
        {
            studentsQuery = studentsQuery.Where(s => s.DepartmentId == query.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            studentsQuery = studentsQuery.Where(s => s.Status == status);
        }

        var students = await studentsQuery.ToListAsync();
        var studentIds = students.Select(s => s.StudentId).ToList();

        var records = await _db.AcademicRecords
            .Include(ar => ar.Semester)
            .Where(ar => studentIds.Contains(ar.StudentId))
            .ToListAsync();

        var latestByStudent = records
            .GroupBy(ar => ar.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(ar => ar.Semester.StartDate).First());

        var items = students.Select(s =>
        {
            latestByStudent.TryGetValue(s.StudentId, out var latest);
            return new StudentListItemResponse
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                StudentNumber = s.StudentNumber,
                FullName = s.FullName,
                Email = s.User.Email,
                Username = s.User.Username,
                Phone = s.Phone,
                DepartmentId = s.DepartmentId,
                DepartmentCode = s.Department.DepartmentCode,
                DepartmentName = s.Department.DepartmentName,
                EnrollmentDate = s.EnrollmentDate,
                Status = s.Status,
                IsActive = s.User.IsActive,
                Cgpa = latest?.CumulativeGpa ?? 0m,
                LatestSemesterName = latest?.Semester.SemesterName
            };
        }).ToList();

        var sortBy = (query.SortBy ?? "name").Trim().ToLowerInvariant();
        var desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        items = sortBy switch
        {
            "studentnumber" => desc
                ? items.OrderByDescending(i => i.StudentNumber).ToList()
                : items.OrderBy(i => i.StudentNumber).ToList(),
            "cgpa" => desc
                ? items.OrderByDescending(i => i.Cgpa).ThenBy(i => i.StudentId).ToList()
                : items.OrderBy(i => i.Cgpa).ThenBy(i => i.StudentId).ToList(),
            _ => desc
                ? items.OrderByDescending(i => i.FullName).ToList()
                : items.OrderBy(i => i.FullName).ToList()
        };

        var totalCount = items.Count;
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var paged = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return (paged, totalCount);
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
