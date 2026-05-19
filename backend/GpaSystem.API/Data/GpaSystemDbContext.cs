using Microsoft.EntityFrameworkCore;
using GpaSystem.API.Models;

namespace GpaSystem.API.Data;

public class GpaSystemDbContext : DbContext
{
    public GpaSystemDbContext(DbContextOptions<GpaSystemDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Department> Departments { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<GradingPolicy> GradingPolicies { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<GradeComponent> GradeComponents { get; set; }
    public DbSet<GradeEntry> GradeEntries { get; set; }
    public DbSet<CourseGrade> CourseGrades { get; set; }
    public DbSet<AcademicRecord> AcademicRecords { get; set; }
    public DbSet<Attendance> Attendances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId);
            entity.Property(e => e.DepartmentCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.DepartmentName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
        });

        // Semester configuration
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId);
            entity.Property(e => e.SemesterName).IsRequired().HasMaxLength(50);
        });

        // GradingPolicy configuration
        modelBuilder.Entity<GradingPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId);
            entity.Property(e => e.LetterGrade).IsRequired().HasMaxLength(3);
            entity.Property(e => e.MinPercentage).HasPrecision(5, 2);
            entity.Property(e => e.MaxPercentage).HasPrecision(5, 2);
            entity.Property(e => e.GradePoint).HasPrecision(3, 2);
        });

        // AppUser configuration
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Administrator configuration
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.AdminId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.User).WithOne(u => u.Administrator)
                .HasForeignKey<Administrator>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Instructor configuration
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.User).WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department).WithMany(d => d.Instructors)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.User).WithOne(u => u.Student)
                .HasForeignKey<Student>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department).WithMany(d => d.Students)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.StudentNumber).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CourseTitle).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Department).WithMany(d => d.Courses)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CourseCode).IsUnique();
        });

        // CoursePrerequisite configuration
        modelBuilder.Entity<CoursePrerequisite>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.PrerequisiteCourseId });
            entity.HasOne(e => e.Course).WithMany(c => c.DependentCourses)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PrerequisiteCourse).WithMany(c => c.Prerequisites)
                .HasForeignKey(e => e.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CourseOffering configuration
        modelBuilder.Entity<CourseOffering>(entity =>
        {
            entity.HasKey(e => e.OfferingId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Course).WithMany(c => c.CourseOfferings)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Semester).WithMany(s => s.CourseOfferings)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Instructor).WithMany(i => i.CourseOfferings)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.CourseId, e.SemesterId, e.InstructorId }).IsUnique();
        });

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Student).WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CourseOffering).WithMany(co => co.Enrollments)
                .HasForeignKey(e => e.OfferingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OriginalEnrollment).WithMany(e => e.RepeatedEnrollments)
                .HasForeignKey(e => e.OriginalEnrollmentId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.StudentId, e.OfferingId }).IsUnique();
        });

        // GradeComponent configuration
        modelBuilder.Entity<GradeComponent>(entity =>
        {
            entity.HasKey(e => e.ComponentId);
            entity.Property(e => e.ComponentName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MaxPoints).HasPrecision(6, 2);
            entity.HasOne(e => e.CourseOffering).WithMany(co => co.GradeComponents)
                .HasForeignKey(e => e.OfferingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OfferingId, e.ComponentName }).IsUnique();
        });

        // GradeEntry configuration
        modelBuilder.Entity<GradeEntry>(entity =>
        {
            entity.HasKey(e => e.GradeEntryId);
            entity.Property(e => e.ObtainedMarks).HasPrecision(6, 2);
            entity.HasOne(e => e.Enrollment).WithMany(en => en.GradeEntries)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.GradeComponent).WithMany(gc => gc.GradeEntries)
                .HasForeignKey(e => e.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Instructor).WithMany(i => i.GradeEntries)
                .HasForeignKey(e => e.RecordedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.EnrollmentId, e.ComponentId }).IsUnique();
        });

        // CourseGrade configuration
        modelBuilder.Entity<CourseGrade>(entity =>
        {
            entity.HasKey(e => e.GradeId);
            entity.Property(e => e.TotalObtained).HasPrecision(8, 2);
            entity.Property(e => e.MaxPossible).HasPrecision(8, 2);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            entity.Property(e => e.LetterGrade).IsRequired().HasMaxLength(3);
            entity.Property(e => e.GradePoints).HasPrecision(4, 2);
            entity.HasOne(e => e.Enrollment).WithOne(en => en.CourseGrade)
                .HasForeignKey<CourseGrade>(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.EnrollmentId).IsUnique();
        });

        // AcademicRecord configuration
        modelBuilder.Entity<AcademicRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);
            entity.Property(e => e.SemesterGpa).HasPrecision(4, 2);
            entity.Property(e => e.CumulativeGpa).HasPrecision(4, 2);
            entity.Property(e => e.TotalGradePoints).HasPrecision(8, 2);
            entity.HasOne(e => e.Student).WithMany(s => s.AcademicRecords)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Semester).WithMany(s => s.AcademicRecords)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.StudentId, e.SemesterId }).IsUnique();
        });

        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(10);
            entity.HasOne(e => e.CourseOffering).WithMany(co => co.Attendances)
                .HasForeignKey(e => e.OfferingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student).WithMany(s => s.Attendances)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Instructor).WithMany(i => i.Attendances)
                .HasForeignKey(e => e.RecordedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
