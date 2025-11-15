using v2.Models;
using v2.Security;
using System.Security.Cryptography;
using System.Text;
using v2.Data;
using Microsoft.EntityFrameworkCore;

namespace v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly Dictionary<string, string> _sessions = new();

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check username uniqueness
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists.");
            if (request.Email != null && await _db.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists.");

            // Hash password
            var hashed = PasswordHelper.HashPassword(request.Password);

            // Create user object
            var user = new UserProfile
            {
                Username = request.Username,
                Password = hashed,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,          // ✅ now filled
                BirthYear = request.BirthYear,
                CreatedAt = DateTime.UtcNow
                    .AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond)),                Active = true,
                Role = "USER"
            };


            // Save in database
            _db.Users.Add(user);
            await _db.SaveChangesAsync();


            // Create session token
            var token = GenerateToken(user.Username);
            _sessions[token] = user.Username;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            var token = GenerateToken(user.Username);
            _sessions[token] = user.Username;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        private static string GenerateToken(string username)
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + username;
    }
}

namespace v2.Security
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string input, string storedHash)
            => HashPassword(input) == storedHash;
    }
}
