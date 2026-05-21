namespace GpaSystem.API.Services;

public interface IPasswordService
{
    string GenerateTemporaryPassword();
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    void ValidatePasswordComplexity(string password);
}
