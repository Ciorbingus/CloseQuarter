using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

using CloseQuarter.Server.Data;
using CloseQuarter.Server.Models;
using CloseQuarter.Shared.DTOs;

namespace CloseQuarter.Server.Services;

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthResponseDTO> LoginOrRegisterAsync(LoginRequestDTO request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            user = new UserEntity
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            _db.Users.Add(user);
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponseDTO { Success = false, Message = "Invalid Password!" };
            }
        }

        string token = string.Empty;
        if (request.RememberMe)
        {
            token = Guid.NewGuid().ToString("N");
            user.SessionToken = token;
            user.SessionTokenExpiry = DateTime.UtcNow.AddDays(30); 
        }

        await _db.SaveChangesAsync();

        return new AuthResponseDTO
        {
            Success = true,
            Message = "Failed Authentification!",
            Username = user.Username,
            SessionToken = token
        };
    }

    public async Task<AuthResponseDTO> LoginWithTokenAsync(TokenLoginRequestDTO request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => 
            u.SessionToken == request.SessionToken && 
            u.SessionTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            return new AuthResponseDTO { Success = false, Message = "Invalid or expired token!" };
        }

        return new AuthResponseDTO
        {
            Success = true,
            Message = "Logged in through Remember Me!",
            Username = user.Username,
            SessionToken = user.SessionToken!
        };
    }
}