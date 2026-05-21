using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface ICourseGradeRepository
{
    Task<CourseGrade?> GetByEnrollmentIdAsync(int enrollmentId);
    Task<List<CourseGrade>> GetForStudentAsync(int studentId);
    Task AddAsync(CourseGrade grade);
    Task SaveChangesAsync();
}
