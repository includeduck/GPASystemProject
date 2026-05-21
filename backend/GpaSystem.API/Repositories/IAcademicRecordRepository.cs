using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IAcademicRecordRepository
{
    Task<AcademicRecord?> GetByStudentAndSemesterAsync(int studentId, int semesterId);
    Task<List<AcademicRecord>> GetForStudentAsync(int studentId);
    Task<List<AcademicRecord>> GetForSemesterAsync(int semesterId);
    Task AddAsync(AcademicRecord record);
    Task SaveChangesAsync();
}
