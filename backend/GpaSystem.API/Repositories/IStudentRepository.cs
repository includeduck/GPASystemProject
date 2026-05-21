using GpaSystem.API.DTOs;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<(List<StudentListItemResponse> Items, int TotalCount)> SearchAsync(StudentSearchQuery query);
    Task<Student?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task<int> CountByStudentNumberPrefixAsync(string prefix);
    Task<bool> StudentNumberExistsAsync(string studentNumber);
    Task AddAsync(Student student);
    Task SaveChangesAsync();
}
