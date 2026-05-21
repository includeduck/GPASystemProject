using System.Text;
using GpaSystem.API.Models;
using GpaSystem.API.Services;

namespace GpaSystem.API.Tests;

public class ReportExportServiceTests
{
    [Fact]
    public async Task ExportTranscriptCsv_ContainsHeaderAndStudentRow()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        db.AcademicRecords.Add(new AcademicRecord
        {
            StudentId = catalog.Student.StudentId,
            SemesterId = catalog.Semester.SemesterId,
            SemesterGpa = 3.00m,
            CumulativeGpa = 3.00m,
            TotalCreditsAttempted = 3,
            TotalGradePoints = 9m,
            CalculationDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var exports = new ReportExportService(ServiceFactory.CreateReportService(db));
        var file = await exports.ExportTranscriptCsvAsync(catalog.Student.StudentId);
        var text = Encoding.UTF8.GetString(file.Content);

        Assert.Contains("StudentNumber,FullName", text);
        Assert.Contains(catalog.Student.StudentNumber, text);
        Assert.EndsWith(".csv", file.FileName);
    }
}
