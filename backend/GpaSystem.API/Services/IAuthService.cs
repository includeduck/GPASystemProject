using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);
    Task<AuthUserResponse> GetCurrentUserAsync(int userId);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, string? ipAddress);
    Task<TemporaryCredentialsResponse> ResetPasswordAsync(int userId, int adminUserId, string? ipAddress);
    Task<CreateAdminResponse> BootstrapAdminAsync(BootstrapAdminRequest? request, string? ipAddress);
}

public class CreateAdminResponse
{
    public AuthUserResponse User { get; set; } = new();
    public TemporaryCredentialsResponse Credentials { get; set; } = new();
}
