using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class AcademicRecordRepository : IAcademicRecordRepository
{
    private readonly GpaSystemDbContext _db;

    public AcademicRecordRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<AcademicRecord?> GetByStudentAndSemesterAsync(int studentId, int semesterId)
    {
        return _db.AcademicRecords
            .FirstOrDefaultAsync(ar => ar.StudentId == studentId && ar.SemesterId == semesterId);
    }

    public Task<List<AcademicRecord>> GetForStudentAsync(int studentId)
    {
        return _db.AcademicRecords
            .Where(ar => ar.StudentId == studentId)
            .Include(ar => ar.Semester)
            .OrderBy(ar => ar.Semester.StartDate)
            .ToListAsync();
    }

    public Task AddAsync(AcademicRecord record)
    {
        return _db.AcademicRecords.AddAsync(record).AsTask();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
