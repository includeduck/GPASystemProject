using System.Security.Cryptography;
using System.Text;
using GpaSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class CredentialService : ICredentialService
{
    private const int Pbkdf2Iterations = 100_000;
    private static readonly char[] PasswordChars =
        "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%".ToCharArray();

    private readonly GpaSystemDbContext _db;

    public CredentialService(GpaSystemDbContext db)
    {
        _db = db;
    }

    public async Task<GeneratedCredentials> GenerateAsync(string fullName, string email)
    {
        var username = await GenerateUsernameAsync(fullName, email);
        var password = GenerateTemporaryPassword();
        return new GeneratedCredentials(username, password, HashPassword(password));
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

    private static string GenerateTemporaryPassword()
    {
        Span<byte> randomBytes = stackalloc byte[14];
        RandomNumberGenerator.Fill(randomBytes);

        var password = new char[randomBytes.Length];
        for (var index = 0; index < randomBytes.Length; index++)
        {
            password[index] = PasswordChars[randomBytes[index] % PasswordChars.Length];
        }

        return new string(password);
    }

    private static string HashPassword(string password)
    {
        Span<byte> salt = stackalloc byte[16];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2-SHA256:{Pbkdf2Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
