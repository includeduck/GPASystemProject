using System.Text;
using GpaSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class CredentialService : ICredentialService
{
    private readonly GpaSystemDbContext _db;
    private readonly IPasswordService _passwords;

    public CredentialService(GpaSystemDbContext db, IPasswordService passwords)
    {
        _db = db;
        _passwords = passwords;
    }

    public async Task<GeneratedCredentials> GenerateAsync(string fullName, string email)
    {
        var username = await GenerateUsernameAsync(fullName, email);
        var password = _passwords.GenerateTemporaryPassword();
        return new GeneratedCredentials(username, password, _passwords.HashPassword(password));
    }

    private async Task<string> GenerateUsernameAsync(string fullName, string email)
    {
        var baseUsername = BuildBaseUsername(fullName);
        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            baseUsername = BuildBaseUsername(email.Split('@')[0]);
        }

        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            baseUsername = "user";
        }

        baseUsername = baseUsername.Length > 42 ? baseUsername[..42] : baseUsername;
        var candidate = baseUsername;
        var suffix = 1;

        while (await _db.AppUsers.AnyAsync(u => u.Username == candidate))
        {
            var suffixText = suffix.ToString();
            var maxBaseLength = Math.Min(baseUsername.Length, 50 - suffixText.Length);
            candidate = $"{baseUsername[..maxBaseLength]}{suffixText}";
            suffix++;
        }

        return candidate;
    }

    private static string BuildBaseUsername(string value)
    {
        var parts = value
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var joined = parts.Length > 0 ? string.Join('.', parts) : value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();

        foreach (var character in joined)
        {
            if (char.IsLetterOrDigit(character) || character == '.')
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Trim('.');
    }

}
