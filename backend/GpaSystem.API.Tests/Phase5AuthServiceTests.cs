using GpaSystem.API.DTOs;
using GpaSystem.API.Data;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class Phase5AuthServiceTests
{
    [Fact]
    public void PasswordService_HashesVerifiesAndRejectsWeakPasswords()
    {
        var service = ServiceFactory.CreatePasswordService();

        var hash = service.HashPassword("Strong@123");

        Assert.StartsWith("PBKDF2-SHA256:100000:", hash);
        Assert.True(service.VerifyPassword("Strong@123", hash));
        Assert.False(service.VerifyPassword("Wrong@123", hash));
        Assert.Throws<ApiException>(() => service.ValidatePasswordComplexity("weak"));

        var temporaryPassword = service.GenerateTemporaryPassword();
        service.ValidatePasswordComplexity(temporaryPassword);
    }

    [Fact]
    public async Task AuthService_LoginAsync_ReturnsJwtAndRecordsLogin()
    {
        await using var database = await TestDatabase.CreateAsync();
        var passwordService = ServiceFactory.CreatePasswordService();
        var auth = ServiceFactory.CreateAuthService(database.Context);
        var user = await AddAdminAsync(database.Context, passwordService.HashPassword("Strong@123"));

        var response = await auth.LoginAsync(
            new LoginRequest { Username = user.Username, Password = "Strong@123" },
            "127.0.0.1");

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(AuthRoles.Admin, response.User.Role);
        Assert.NotNull(await database.Context.AppUsers.Where(u => u.UserId == user.UserId).Select(u => u.LastLogin).FirstAsync());
        Assert.True(await database.Context.AuditLog.AnyAsync(l => l.UserId == user.UserId && l.ActionType == "LOGIN"));
    }

    [Fact]
    public async Task AuthService_LoginAsync_RejectsInvalidOrInactiveAccounts()
    {
        await using var database = await TestDatabase.CreateAsync();
        var passwordService = ServiceFactory.CreatePasswordService();
        var auth = ServiceFactory.CreateAuthService(database.Context);
        var user = await AddAdminAsync(database.Context, passwordService.HashPassword("Strong@123"));

        var invalid = await Assert.ThrowsAsync<ApiException>(() => auth.LoginAsync(
            new LoginRequest { Username = user.Username, Password = "Wrong@123" },
            null));
        Assert.Equal(401, invalid.StatusCode);

        user.IsActive = false;
        await database.Context.SaveChangesAsync();
        var inactive = await Assert.ThrowsAsync<ApiException>(() => auth.LoginAsync(
            new LoginRequest { Username = user.Username, Password = "Strong@123" },
            null));
        Assert.Equal(401, inactive.StatusCode);
    }

    [Fact]
    public async Task AuthService_ChangePasswordAsync_UpdatesHashAndAllowsNewLogin()
    {
        await using var database = await TestDatabase.CreateAsync();
        var passwordService = ServiceFactory.CreatePasswordService();
        var auth = ServiceFactory.CreateAuthService(database.Context);
        var user = await AddAdminAsync(database.Context, passwordService.HashPassword("Strong@123"));

        await auth.ChangePasswordAsync(
            user.UserId,
            new ChangePasswordRequest { CurrentPassword = "Strong@123", NewPassword = "Better@123" },
            null);

        await Assert.ThrowsAsync<ApiException>(() => auth.LoginAsync(
            new LoginRequest { Username = user.Username, Password = "Strong@123" },
            null));
        var response = await auth.LoginAsync(
            new LoginRequest { Username = user.Username, Password = "Better@123" },
            null);
        Assert.Equal(user.UserId, response.User.UserId);
    }

    [Fact]
    public async Task AuthService_ResetPasswordAsync_ReturnsTemporaryPassword()
    {
        await using var database = await TestDatabase.CreateAsync();
        var passwordService = ServiceFactory.CreatePasswordService();
        var auth = ServiceFactory.CreateAuthService(database.Context);
        var admin = await AddAdminAsync(database.Context, passwordService.HashPassword("Strong@123"));
        var target = await AddAdminAsync(database.Context, passwordService.HashPassword("OldPass@123"), "target.admin", "target@example.edu");

        var credentials = await auth.ResetPasswordAsync(target.UserId, admin.UserId, "127.0.0.1");
        var updatedHash = await database.Context.AppUsers
            .Where(u => u.UserId == target.UserId)
            .Select(u => u.PasswordHash)
            .FirstAsync();

        Assert.Equal(target.Username, credentials.Username);
        Assert.True(passwordService.VerifyPassword(credentials.TemporaryPassword, updatedHash));
        Assert.True(await database.Context.AuditLog.AnyAsync(l =>
            l.UserId == admin.UserId &&
            l.RecordId == target.UserId &&
            l.ActionType == "PASSWORD_RESET"));
    }

    private static async Task<AppUser> AddAdminAsync(
        GpaSystemDbContext db,
        string passwordHash,
        string username = "admin",
        string email = "admin@example.edu")
    {
        var user = new AppUser
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            Role = AuthRoles.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordChangedAt = DateTime.UtcNow,
            Administrator = new Administrator
            {
                FullName = "System Administrator"
            }
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
