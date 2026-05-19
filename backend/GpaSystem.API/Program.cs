using Microsoft.EntityFrameworkCore;
using GpaSystem.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

// Add DbContext
builder.Services.AddDbContext<GpaSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GpaSystemDb")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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

app.Run();
