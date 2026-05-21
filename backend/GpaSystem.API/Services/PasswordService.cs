using System.Security.Cryptography;
using GpaSystem.API.Exceptions;

namespace GpaSystem.API.Services;

public class PasswordService : IPasswordService
{
    private const int Pbkdf2Iterations = 100_000;
    private static readonly char[] PasswordChars =
        "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%".ToCharArray();

    public string GenerateTemporaryPassword()
    {
        while (true)
        {
            var randomBytes = new byte[14];
            RandomNumberGenerator.Fill(randomBytes);

            var password = new char[randomBytes.Length];
            for (var index = 0; index < randomBytes.Length; index++)
            {
                password[index] = PasswordChars[randomBytes[index] % PasswordChars.Length];
            }

            var candidate = new string(password);
            try
            {
                ValidatePasswordComplexity(candidate);
                return candidate;
            }
            catch (ApiException)
            {
                // Extremely unlikely; regenerate until the temporary password meets policy.
            }
        }
    }

    public string HashPassword(string password)
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

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedHash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    public void ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw ApiException.BadRequest("Password must be at least 8 characters long.");
        }

        if (!password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) ||
            !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw ApiException.BadRequest("Password must include uppercase, lowercase, digit, and special characters.");
        }
    }
}
