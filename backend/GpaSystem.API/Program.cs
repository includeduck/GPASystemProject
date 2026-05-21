using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using GpaSystem.API.Data;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Repositories;
using GpaSystem.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

// Add DbContext
builder.Services.AddDbContext<GpaSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GpaSystemDb")));

// Add application services
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseOfferingRepository, CourseOfferingRepository>();
builder.Services.AddScoped<ICoursePrerequisiteRepository, CoursePrerequisiteRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IGradeComponentRepository, GradeComponentRepository>();
builder.Services.AddScoped<IGradeEntryRepository, GradeEntryRepository>();
builder.Services.AddScoped<IGradingPolicyRepository, GradingPolicyRepository>();
builder.Services.AddScoped<ICourseGradeRepository, CourseGradeRepository>();
builder.Services.AddScoped<IAcademicRecordRepository, AcademicRecordRepository>();

builder.Services.AddScoped<ICredentialService, CredentialService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseOfferingService, CourseOfferingService>();
builder.Services.AddScoped<IPrerequisiteService, PrerequisiteService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IGradingStrategy, StandardGradingStrategy>();
builder.Services.AddScoped<IGpaCalculatorService, GpaCalculatorService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IGradingPolicyService, GradingPolicyService>();
builder.Services.AddScoped<DemoDataSeeder>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173",
                  "http://127.0.0.1:5173",
                  "http://localhost:3000",
                  "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        var statusCode = exception is ApiException apiException
            ? apiException.StatusCode
            : StatusCodes.Status500InternalServerError;
        var message = exception is ApiException
            ? exception.Message
            : "An unexpected error occurred while processing the request.";

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message });
    });
});

// Test endpoint - verify API is working
app.MapGet("/api/test", () => new { message = "GPA System API is running!", timestamp = DateTime.UtcNow })
    .WithName("TestApi")
    .WithOpenApi();

// Health check endpoint
app.MapGet("/api/health", async (GpaSystemDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlAsync($"SELECT 1");
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch
    {
        return Results.StatusCode(500);
    }
})
.WithName("HealthCheck")
.WithOpenApi();

app.MapControllers();

app.Run();
