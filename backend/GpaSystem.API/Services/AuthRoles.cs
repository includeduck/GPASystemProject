namespace GpaSystem.API.Services;

public static class AuthRoles
{
    public const string Admin = "ADMIN";
    public const string Instructor = "INSTRUCTOR";
    public const string Student = "STUDENT";

    public const string AdminOrInstructor = Admin + "," + Instructor;
    public const string AdminOrStudent = Admin + "," + Student;
    public const string AdminOrInstructorOrStudent = Admin + "," + Instructor + "," + Student;
}

public static class AuthClaimTypes
{
    public const string StudentId = "student_id";
    public const string InstructorId = "instructor_id";
    public const string AdminId = "admin_id";
    public const string DisplayName = "display_name";
}
