using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GpaSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly GpaSystemDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly IConfiguration _configuration;

    public AuthService(GpaSystemDbContext db, IPasswordService passwords, IConfiguration configuration)
    {
        _db = db;
        _passwords = passwords;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var login = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw ApiException.Unauthorized("Invalid username or password.");
        }

        var normalized = login.ToUpperInvariant();
        var user = await UserQuery()
            .FirstOrDefaultAsync(u => u.Username.ToUpper() == normalized || u.Email.ToUpper() == normalized);

        if (user == null || !user.IsActive || !_passwords.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw ApiException.Unauthorized("Invalid username or password.");
        }

        user.LastLogin = DateTime.UtcNow;
        await AddAuditAsync(user.UserId, "LOGIN", "AppUser", user.UserId, null, "User signed in.", ipAddress);
        await _db.SaveChangesAsync();

        var profile = MapUser(user);
        var (token, expiresAt) = CreateToken(profile);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = profile
        };
    }

    public async Task<AuthUserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await UserQuery().FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw ApiException.Unauthorized("User session is no longer valid.");

        if (!user.IsActive)
        {
            throw ApiException.Unauthorized("User account is inactive.");
        }

        return MapUser(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, string? ipAddress)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw ApiException.Unauthorized("User session is no longer valid.");

        if (!user.IsActive || !_passwords.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw ApiException.Unauthorized("Current password is incorrect.");
        }

        _passwords.ValidatePasswordComplexity(request.NewPassword);
        user.PasswordHash = _passwords.HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;

        await AddAuditAsync(user.UserId, "PASSWORD_CHANGE", "AppUser", user.UserId, null, "Password changed by account owner.", ipAddress);
        await _db.SaveChangesAsync();
    }

    public async Task<TemporaryCredentialsResponse> ResetPasswordAsync(int userId, int adminUserId, string? ipAddress)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw ApiException.NotFound("User was not found.");

        var temporaryPassword = _passwords.GenerateTemporaryPassword();
        user.PasswordHash = _passwords.HashPassword(temporaryPassword);
        user.PasswordChangedAt = DateTime.UtcNow;

        await AddAuditAsync(adminUserId, "PASSWORD_RESET", "AppUser", user.UserId, null, $"Password reset for {user.Username}.", ipAddress);
        await _db.SaveChangesAsync();

        return new TemporaryCredentialsResponse
        {
            Username = user.Username,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<CreateAdminResponse> BootstrapAdminAsync(BootstrapAdminRequest? request, string? ipAddress)
    {
        if (await _db.Administrators.AnyAsync())
        {
            throw ApiException.Conflict("An administrator account already exists.");
        }

        var username = NormalizeUsername(request?.Username ?? "admin");
        var email = (request?.Email ?? "admin@gpasystem.local").Trim().ToLowerInvariant();
        var fullName = string.IsNullOrWhiteSpace(request?.FullName)
            ? "System Administrator"
            : request.FullName.Trim();

        if (await _db.AppUsers.AnyAsync(u => u.Username == username || u.Email == email))
        {
            throw ApiException.Conflict("The requested administrator username or email is already in use.");
        }

        var temporaryPassword = _passwords.GenerateTemporaryPassword();
        var user = new AppUser
        {
            Username = username,
            Email = email,
            PasswordHash = _passwords.HashPassword(temporaryPassword),
            Role = AuthRoles.Admin,
            IsActive = true,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Administrator = new Administrator
            {
                FullName = fullName
            }
        };

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        await AddAuditAsync(user.UserId, "ADMIN_BOOTSTRAP", "AppUser", user.UserId, null, "Development administrator account created.", ipAddress);
        await _db.SaveChangesAsync();

        return new CreateAdminResponse
        {
            User = MapUser(user),
            Credentials = new TemporaryCredentialsResponse
            {
                Username = username,
                TemporaryPassword = temporaryPassword
            }
        };
    }

    private IQueryable<AppUser> UserQuery()
    {
        return _db.AppUsers
            .Include(u => u.Administrator)
            .Include(u => u.Student)
            .Include(u => u.Instructor);
    }

    private (string Token, DateTime ExpiresAt) CreateToken(AuthUserResponse user)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "GpaSystem";
        var audience = _configuration["Jwt:Audience"] ?? "GpaSystemClient";
        var signingKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured and at least 32 characters long.");
        }

        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var configuredExpiry)
            ? configuredExpiry
            : 15;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(AuthClaimTypes.DisplayName, user.DisplayName)
        };

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim(AuthClaimTypes.StudentId, user.StudentId.Value.ToString()));
        }

        if (user.InstructorId.HasValue)
        {
            claims.Add(new Claim(AuthClaimTypes.InstructorId, user.InstructorId.Value.ToString()));
        }

        if (user.AdminId.HasValue)
        {
            claims.Add(new Claim(AuthClaimTypes.AdminId, user.AdminId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static AuthUserResponse MapUser(AppUser user)
    {
        var displayName = user.Role switch
        {
            AuthRoles.Admin => user.Administrator?.FullName,
            AuthRoles.Instructor => user.Instructor?.FullName,
            AuthRoles.Student => user.Student?.FullName,
            _ => user.Username
        } ?? user.Username;

        return new AuthUserResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            DisplayName = displayName,
            AdminId = user.Administrator?.AdminId,
            InstructorId = user.Instructor?.InstructorId,
            StudentId = user.Student?.StudentId
        };
    }

    private async Task AddAuditAsync(
        int userId,
        string actionType,
        string? tableName,
        int? recordId,
        string? oldValue,
        string? newValue,
        string? ipAddress)
    {
        await _db.AuditLog.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionType = actionType,
            TableName = tableName,
            RecordId = recordId,
            OldValue = oldValue,
            NewValue = newValue,
            IpAddress = ipAddress,
            LoggedAt = DateTime.UtcNow
        });
    }

    private static string NormalizeUsername(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw ApiException.BadRequest("Username is required.");
        }

        return normalized.Length > 50 ? normalized[..50] : normalized;
    }
}
