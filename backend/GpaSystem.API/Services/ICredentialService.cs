namespace GpaSystem.API.Services;

public record GeneratedCredentials(string Username, string TemporaryPassword, string PasswordHash);

public interface ICredentialService
{
    Task<GeneratedCredentials> GenerateAsync(string fullName, string email);
}
