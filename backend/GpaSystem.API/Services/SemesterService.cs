using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesters;

    public SemesterService(ISemesterRepository semesters)
    {
        _semesters = semesters;
    }

    public async Task<List<SemesterResponse>> GetAllAsync()
    {
        var semesters = await _semesters.GetAllAsync();
        return semesters.Select(Map).ToList();
    }

    public async Task<SemesterResponse> GetByIdAsync(int id)
    {
        var semester = await FindSemesterAsync(id);
        return Map(semester);
    }

    public async Task<SemesterResponse> CreateAsync(CreateSemesterRequest request)
    {
        ValidateDates(request.StartDate, request.EndDate);

        if (request.IsCurrent)
        {
            await _semesters.ClearCurrentAsync();
        }

        var semester = new Semester
        {
            SemesterName = request.SemesterName.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrent = request.IsCurrent
        };

        await _semesters.AddAsync(semester);
        await _semesters.SaveChangesAsync();
        return Map(semester);
    }

    public async Task<SemesterResponse> UpdateAsync(int id, UpdateSemesterRequest request)
    {
        ValidateDates(request.StartDate, request.EndDate);
        var semester = await FindSemesterAsync(id);

        if (request.IsCurrent)
        {
            await _semesters.ClearCurrentAsync(id);
        }

        semester.SemesterName = request.SemesterName.Trim();
        semester.StartDate = request.StartDate;
        semester.EndDate = request.EndDate;
        semester.IsCurrent = request.IsCurrent;

        await _semesters.SaveChangesAsync();
        return Map(semester);
    }

    public async Task<SemesterResponse> SetCurrentAsync(int id)
    {
        var semester = await FindSemesterAsync(id);
        await _semesters.ClearCurrentAsync(id);
        semester.IsCurrent = true;
        await _semesters.SaveChangesAsync();
        return Map(semester);
    }

    public async Task DeleteAsync(int id)
    {
        var semester = await FindSemesterAsync(id);

        if (await _semesters.HasReferencesAsync(id))
        {
            throw ApiException.Conflict("Semester cannot be deleted while offerings or academic records reference it.");
        }

        _semesters.Remove(semester);
        await _semesters.SaveChangesAsync();
    }

    private async Task<Semester> FindSemesterAsync(int id)
    {
        return await _semesters.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Semester was not found.");
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw ApiException.BadRequest("Semester end date must be after the start date.");
        }
    }

    private static SemesterResponse Map(Semester semester)
    {
        return new SemesterResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            IsCurrent = semester.IsCurrent
        };
    }
}
