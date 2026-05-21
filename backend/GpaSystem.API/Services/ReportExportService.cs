using System.Globalization;
using System.Text;
using GpaSystem.API.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GpaSystem.API.Services;

public class ReportExportService : IReportExportService
{
    private readonly IReportService _reports;

    public ReportExportService(IReportService reports)
    {
        _reports = reports;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ExportedFile> ExportTranscriptCsvAsync(int studentId)
    {
        var transcript = await _reports.GetTranscriptAsync(studentId);
        var sb = new StringBuilder();
        sb.AppendLine("StudentNumber,FullName,Department,CGPA,CreditsAttempted,CreditsEarned");
        sb.AppendLine(CsvRow(
            transcript.StudentNumber,
            transcript.FullName,
            transcript.DepartmentCode,
            transcript.CGPA.ToString("F2", CultureInfo.InvariantCulture),
            transcript.TotalCreditsAttempted.ToString(CultureInfo.InvariantCulture),
            transcript.TotalCreditsEarned.ToString(CultureInfo.InvariantCulture)));

        sb.AppendLine();
        sb.AppendLine("Semester,CourseCode,CourseTitle,CreditHours,Percentage,LetterGrade,GradePoints,Status,Repeated");

        foreach (var semester in transcript.Semesters)
        {
            foreach (var course in semester.Courses)
            {
                sb.AppendLine(CsvRow(
                    semester.SemesterName,
                    course.CourseCode,
                    course.CourseTitle,
                    course.CreditHours.ToString(CultureInfo.InvariantCulture),
                    course.Percentage.ToString("F2", CultureInfo.InvariantCulture),
                    course.LetterGrade,
                    course.GradePoints.ToString("F2", CultureInfo.InvariantCulture),
                    course.Status,
                    course.IsRepeatedAttempt ? "Yes" : "No"));
            }
        }

        return ToCsvFile(sb, $"Transcript_{transcript.StudentNumber}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<ExportedFile> ExportTranscriptPdfAsync(int studentId)
    {
        var transcript = await _reports.GetTranscriptAsync(studentId);
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("Official Academic Transcript").Bold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Item().Text($"{transcript.FullName} ({transcript.StudentNumber})");
                    col.Item().Text($"{transcript.DepartmentName} ({transcript.DepartmentCode})");
                    col.Item().Text($"CGPA: {transcript.CGPA:F2} | Credits: {transcript.TotalCreditsEarned}/{transcript.TotalCreditsAttempted}");
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    foreach (var semester in transcript.Semesters)
                    {
                        col.Item().Text(semester.SemesterName).Bold().FontSize(14);
                        col.Item().Text($"GPA: {semester.GPA:F2} | CGPA: {semester.CGPA:F2}");
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.ConstantColumn(40);
                                c.ConstantColumn(50);
                                c.ConstantColumn(40);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("Code").Bold();
                                h.Cell().Text("Title").Bold();
                                h.Cell().Text("Cr").Bold();
                                h.Cell().Text("Grade").Bold();
                                h.Cell().Text("GP").Bold();
                            });
                            foreach (var course in semester.Courses)
                            {
                                table.Cell().Text(course.CourseCode);
                                table.Cell().Text(course.CourseTitle);
                                table.Cell().Text(course.CreditHours.ToString());
                                table.Cell().Text(course.LetterGrade);
                                table.Cell().Text(course.GradePoints.ToString("F2"));
                            }
                        });
                        col.Item().PaddingBottom(8);
                    }

                    if (transcript.FailedCourses.Count > 0)
                    {
                        col.Item().Text("Failed Courses").Bold().FontColor(Colors.Red.Medium);
                        foreach (var f in transcript.FailedCourses)
                        {
                            col.Item().Text($"{f.CourseCode} - {f.CourseTitle} ({f.LetterGrade})");
                        }
                    }
                });
                page.Footer().AlignCenter().Text($"Generated {transcript.GeneratedAt:yyyy-MM-dd HH:mm} UTC");
            });
        }).GeneratePdf();

        return new ExportedFile
        {
            Content = pdf,
            ContentType = "application/pdf",
            FileName = $"Transcript_{transcript.StudentNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf"
        };
    }

    public async Task<ExportedFile> ExportSemesterResultsCsvAsync(int semesterId)
    {
        var report = await _reports.GetSemesterResultsAsync(semesterId);
        var sb = new StringBuilder();
        sb.AppendLine("StudentNumber,FullName,Department,SemesterGpa,CumulativeGpa,CreditsAttempted");
        foreach (var s in report.Students)
        {
            sb.AppendLine(CsvRow(
                s.StudentNumber,
                s.FullName,
                s.DepartmentCode,
                s.SemesterGpa.ToString("F2", CultureInfo.InvariantCulture),
                s.CumulativeGpa.ToString("F2", CultureInfo.InvariantCulture),
                s.CreditsAttempted.ToString(CultureInfo.InvariantCulture)));
        }

        return ToCsvFile(sb, $"SemesterResults_{report.SemesterName}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<ExportedFile> ExportWarningsCsvAsync(int semesterId, decimal? threshold)
    {
        var report = await _reports.GetWarningListAsync(semesterId, threshold);
        var sb = new StringBuilder();
        sb.AppendLine($"Threshold,{report.Threshold:F2}");
        sb.AppendLine("StudentNumber,FullName,Department,SemesterGpa,CumulativeGpa");
        foreach (var s in report.Students)
        {
            sb.AppendLine(CsvRow(
                s.StudentNumber,
                s.FullName,
                s.DepartmentCode,
                s.SemesterGpa.ToString("F2", CultureInfo.InvariantCulture),
                s.CumulativeGpa.ToString("F2", CultureInfo.InvariantCulture)));
        }

        return ToCsvFile(sb, $"WarningList_{report.SemesterName}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<ExportedFile> ExportRankingsCsvAsync(int? departmentId, int? semesterId)
    {
        var report = await _reports.GetClassRankingsAsync(departmentId, semesterId);
        var sb = new StringBuilder();
        sb.AppendLine("Rank,StudentNumber,FullName,Department,CGPA");
        foreach (var r in report.Rankings)
        {
            sb.AppendLine(CsvRow(
                r.Rank.ToString(CultureInfo.InvariantCulture),
                r.StudentNumber,
                r.FullName,
                r.DepartmentCode,
                r.Cgpa.ToString("F2", CultureInfo.InvariantCulture)));
        }

        return ToCsvFile(sb, $"ClassRankings_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<ExportedFile> ExportCoursePerformanceCsvAsync(int courseId, int? semesterId)
    {
        var report = await _reports.GetCoursePerformanceAsync(courseId, semesterId);
        var sb = new StringBuilder();
        sb.AppendLine("CourseCode,CourseTitle,TotalEnrollments,Passed,Failed,AveragePercentage");
        sb.AppendLine(CsvRow(
            report.CourseCode,
            report.CourseTitle,
            report.TotalEnrollments.ToString(CultureInfo.InvariantCulture),
            report.PassedCount.ToString(CultureInfo.InvariantCulture),
            report.FailedCount.ToString(CultureInfo.InvariantCulture),
            report.AveragePercentage.ToString("F2", CultureInfo.InvariantCulture)));
        sb.AppendLine();
        sb.AppendLine("OfferingId,Semester,Instructor,Enrollments,AvgPercentage");
        foreach (var o in report.Offerings)
        {
            sb.AppendLine(CsvRow(
                o.OfferingId.ToString(CultureInfo.InvariantCulture),
                o.SemesterName,
                o.InstructorName,
                o.EnrollmentCount.ToString(CultureInfo.InvariantCulture),
                o.AveragePercentage.ToString("F2", CultureInfo.InvariantCulture)));
        }

        return ToCsvFile(sb, $"CoursePerformance_{report.CourseCode}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<ExportedFile> ExportDepartmentPerformanceCsvAsync(int departmentId, int? semesterId)
    {
        var report = await _reports.GetDepartmentPerformanceAsync(departmentId, semesterId);
        var sb = new StringBuilder();
        sb.AppendLine("DepartmentCode,DepartmentName,StudentCount,AvgSemesterGpa,PassRate");
        sb.AppendLine(CsvRow(
            report.DepartmentCode,
            report.DepartmentName,
            report.StudentCount.ToString(CultureInfo.InvariantCulture),
            report.AverageSemesterGpa.ToString("F2", CultureInfo.InvariantCulture),
            report.PassRate.ToString("F2", CultureInfo.InvariantCulture)));
        sb.AppendLine();
        sb.AppendLine("StudentNumber,FullName,SemesterGpa,CumulativeGpa");
        foreach (var s in report.Students)
        {
            sb.AppendLine(CsvRow(
                s.StudentNumber,
                s.FullName,
                s.SemesterGpa.ToString("F2", CultureInfo.InvariantCulture),
                s.CumulativeGpa.ToString("F2", CultureInfo.InvariantCulture)));
        }

        return ToCsvFile(sb, $"DepartmentPerformance_{report.DepartmentCode}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static ExportedFile ToCsvFile(StringBuilder sb, string fileName) =>
        new()
        {
            Content = Encoding.UTF8.GetBytes(sb.ToString()),
            ContentType = "text/csv",
            FileName = fileName
        };

    private static string CsvRow(params string[] values) =>
        string.Join(",", values.Select(EscapeCsv));

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
