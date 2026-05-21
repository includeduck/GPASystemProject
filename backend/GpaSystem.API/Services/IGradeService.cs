using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IGradeService
{
    Task<List<GradeComponentResponse>> GetComponentsAsync(int offeringId);
    Task<GradeComponentResponse> CreateComponentAsync(int offeringId, CreateGradeComponentRequest request);
    Task<GradeComponentResponse> UpdateComponentAsync(int id, UpdateGradeComponentRequest request);
    Task DeleteComponentAsync(int id);

    Task<List<RosterGradeResponse>> GetGradebookRosterAsync(int offeringId);
    Task RecordMarksAsync(int offeringId, List<RecordGradeEntryRequest> requests, int instructorId);
    Task FinalizeGradesAsync(int offeringId, bool force, int instructorId);
}
