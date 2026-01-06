using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Avemepls.Auth.Password.Abstractions;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Avemepls.Auth.Password;

public class AuthService(IHttpContextAccessor httpContextAccessor) : IAuthService
{
    // В реальном проекте здесь был бы репозиторий для работы с БД
    // Для примера используем временное хранилище
    private static readonly List<User> _users = [];

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = _users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);

        if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return user;
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        if (_users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Пользователь с таким именем уже существует");
        }

        if (_users.Exists(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(password),
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        _users.Add(user);

        // Автоматическая аутентификация после регистрации
        await AuthenticateAsync(username, password);

        return user;
    }

    public Task<bool> ChangePasswordAsync(Id<User> userId, string oldPassword, string newPassword)
    {
        var user = _users.Find(u => u.Id == userId && u.IsActive);

        if (user == null || !PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
        {
            return Task.FromResult(false);
        }

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.DateUpdated = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    public async Task LogoutAsync()
    {
        await httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}