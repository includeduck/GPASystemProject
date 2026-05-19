using Microsoft.EntityFrameworkCore;
using GpaSystem.API.Models;

namespace GpaSystem.API.Data;

public class GpaSystemDbContext : DbContext
{
    public GpaSystemDbContext(DbContextOptions<GpaSystemDbContext> options) : base(options)
    {
    }

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

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.HasKey(e => e.DepartmentId);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentCode).HasColumnName("department_code").IsRequired().HasMaxLength(10);
            entity.Property(e => e.DepartmentName).HasColumnName("department_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester");
            entity.HasKey(e => e.SemesterId);
            entity.Property(e => e.SemesterId).HasColumnName("semester_id");
            entity.Property(e => e.SemesterName).HasColumnName("semester_name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsCurrent).HasColumnName("is_current");
        });

        modelBuilder.Entity<GradingPolicy>(entity =>
        {
            entity.ToTable("GradingPolicy");
            entity.HasKey(e => e.PolicyId);
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.LetterGrade).HasColumnName("letter_grade").IsRequired().HasMaxLength(3);
            entity.Property(e => e.MinPercentage).HasColumnName("min_percentage").HasPrecision(5, 2);
            entity.Property(e => e.MaxPercentage).HasColumnName("max_percentage").HasPrecision(5, 2);
            entity.Property(e => e.GradePoint).HasColumnName("grade_point").HasPrecision(3, 2);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.PasswordChangedAt).HasColumnName("password_changed_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.ToTable("Administrator");
            entity.HasKey(e => e.AdminId);
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.User).WithOne(u => u.Administrator)
                .HasForeignKey<Administrator>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable("Instructor");
            entity.HasKey(e => e.InstructorId);
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.HasOne(e => e.User).WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department).WithMany(d => d.Instructors)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StudentNumber).HasColumnName("student_number").IsRequired().HasMaxLength(20);
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.User).WithOne(u => u.Student)
                .HasForeignKey<Student>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department).WithMany(d => d.Students)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.StudentNumber).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CourseCode).HasColumnName("course_code").IsRequired().HasMaxLength(20);
            entity.Property(e => e.CourseTitle).HasColumnName("course_title").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreditHours).HasColumnName("credit_hours");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Department).WithMany(d => d.Courses)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CourseCode).IsUnique();
        });

        modelBuilder.Entity<CoursePrerequisite>(entity =>
        {
            entity.ToTable("CoursePrerequisite");
            entity.HasKey(e => new { e.CourseId, e.PrerequisiteCourseId });
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.PrerequisiteCourseId).HasColumnName("prerequisite_course_id");
            entity.HasOne(e => e.Course).WithMany(c => c.DependentCourses)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PrerequisiteCourse).WithMany(c => c.Prerequisites)
                .HasForeignKey(e => e.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CourseOffering>(entity =>
        {
            entity.ToTable("CourseOffering");
            entity.HasKey(e => e.OfferingId);
            entity.Property(e => e.OfferingId).HasColumnName("offering_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SemesterId).HasColumnName("semester_id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.MaxCapacity).HasColumnName("max_capacity");
            entity.Property(e => e.CurrentEnrollment).HasColumnName("current_enrollment");
            entity.Property(e => e.IsGradeFinalized).HasColumnName("is_grade_finalized");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
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

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollment");
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.OfferingId).HasColumnName("offering_id");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsRepeated).HasColumnName("is_repeated");
            entity.Property(e => e.OriginalEnrollmentId).HasColumnName("original_enrollment_id");
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

        modelBuilder.Entity<GradeComponent>(entity =>
        {
            entity.ToTable("GradeComponent");
            entity.HasKey(e => e.ComponentId);
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.OfferingId).HasColumnName("offering_id");
            entity.Property(e => e.ComponentName).HasColumnName("component_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.MaxPoints).HasColumnName("max_points").HasPrecision(6, 2);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.HasOne(e => e.CourseOffering).WithMany(co => co.GradeComponents)
                .HasForeignKey(e => e.OfferingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OfferingId, e.ComponentName }).IsUnique();
        });

        modelBuilder.Entity<GradeEntry>(entity =>
        {
            entity.ToTable("GradeEntry");
            entity.HasKey(e => e.GradeEntryId);
            entity.Property(e => e.GradeEntryId).HasColumnName("grade_entry_id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.ObtainedMarks).HasColumnName("obtained_marks").HasPrecision(6, 2);
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastModifiedAt).HasColumnName("last_modified_at").HasDefaultValueSql("GETUTCDATE()");
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

        modelBuilder.Entity<CourseGrade>(entity =>
        {
            entity.ToTable("CourseGrade");
            entity.HasKey(e => e.GradeId);
            entity.Property(e => e.GradeId).HasColumnName("grade_id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.TotalObtained).HasColumnName("total_obtained").HasPrecision(8, 2);
            entity.Property(e => e.MaxPossible).HasColumnName("max_possible").HasPrecision(8, 2);
            entity.Property(e => e.Percentage).HasColumnName("percentage").HasPrecision(5, 2);
            entity.Property(e => e.LetterGrade).HasColumnName("letter_grade").IsRequired().HasMaxLength(3);
            entity.Property(e => e.GradePoints).HasColumnName("grade_points").HasPrecision(4, 2);
            entity.Property(e => e.IsRepeatedAttempt).HasColumnName("is_repeated_attempt");
            entity.Property(e => e.CalculatedAt).HasColumnName("calculated_at").HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Enrollment).WithOne(en => en.CourseGrade)
                .HasForeignKey<CourseGrade>(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.EnrollmentId).IsUnique();
        });

        modelBuilder.Entity<AcademicRecord>(entity =>
        {
            entity.ToTable("AcademicRecord");
            entity.HasKey(e => e.RecordId);
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SemesterId).HasColumnName("semester_id");
            entity.Property(e => e.SemesterGpa).HasColumnName("semester_gpa").HasPrecision(4, 2);
            entity.Property(e => e.CumulativeGpa).HasColumnName("cumulative_gpa").HasPrecision(4, 2);
            entity.Property(e => e.TotalCreditsAttempted).HasColumnName("total_credits_attempted");
            entity.Property(e => e.TotalGradePoints).HasColumnName("total_grade_points").HasPrecision(8, 2);
            entity.Property(e => e.CalculationDate).HasColumnName("calculation_date").HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Student).WithMany(s => s.AcademicRecords)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Semester).WithMany(s => s.AcademicRecords)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.StudentId, e.SemesterId }).IsUnique();
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendance");
            entity.HasKey(e => e.AttendanceId);
            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.OfferingId).HasColumnName("offering_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(10);
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at").HasDefaultValueSql("GETUTCDATE()");
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
