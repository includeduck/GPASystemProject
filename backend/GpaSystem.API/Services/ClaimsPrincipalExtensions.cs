using System.Security.Claims;

namespace GpaSystem.API.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }

    public static int? GetStudentId(this ClaimsPrincipal user)
    {
        return TryGetIntClaim(user, AuthClaimTypes.StudentId);
    }

    public static int? GetInstructorId(this ClaimsPrincipal user)
    {
        return TryGetIntClaim(user, AuthClaimTypes.InstructorId);
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(AuthRoles.Admin);
    }

    private static int? TryGetIntClaim(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirstValue(claimType);
        return int.TryParse(value, out var id) ? id : null;
    }
}
