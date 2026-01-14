using v2.Models;
using v2.Security;
using System.Security.Cryptography;
using System.Text;
using v2.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        // token → username
        private static readonly Dictionary<string, string> _sessions = new();

        // username → token
        private static readonly Dictionary<string, string> _userSessions = new();

        // currently logged-in user's token (for automatic logout)
        private static string? _currentUserToken;

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        // REGISTER
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var context = new ValidationContext(request);
            Validator.ValidateObject(request, context, validateAllProperties: true);

            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists.");

            if (!string.IsNullOrWhiteSpace(request.Email) &&
                await _db.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists.");

            var hashedPassword = PasswordHelper.HashPassword(request.Password);

            var user = new UserProfile
            {
                Username = request.Username,
                Password = hashedPassword,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                BirthYear = request.BirthYear,
                CreatedAt = DateTime.UtcNow
                    .AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond)),
                Active = true,
                Role = "USER"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateToken(user.Username);

            if (_userSessions.TryGetValue(user.Username, out var oldToken))
            {
                _sessions.Remove(oldToken);
                _userSessions.Remove(user.Username);
            }

            _sessions[token] = user.Username;
            _userSessions[user.Username] = token;
            _currentUserToken = token;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }


        // LOGIN
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            // Ensure only one session per user
            if (_userSessions.TryGetValue(user.Username, out var oldToken))
            {
                _sessions.Remove(oldToken);
                _userSessions.Remove(user.Username);
            }

            var token = GenerateToken(user.Username);

            _sessions[token] = user.Username;
            _userSessions[user.Username] = token;

            // Set current user token for automatic logout
            _currentUserToken = token;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        // LOGOUT (manual token)
        public async Task<bool> LogoutAsync(string token)
        {
            token = Uri.UnescapeDataString(token);

            if (!_sessions.ContainsKey(token))
                return false;

            var username = _sessions[token];

            if (!_userSessions.TryGetValue(username, out var currentToken))
                return false;

            if (currentToken != token)
                return false;

            _sessions.Remove(token);
            _userSessions.Remove(username);

            // Clear current token if it matches
            if (_currentUserToken == token)
                _currentUserToken = null;

            return true;
        }

        // LOGOUT current user (automatic)
        public async Task<bool> LogoutCurrentUserAsync()
        {
            if (_currentUserToken == null)
                return false; // no active user

            var token = _currentUserToken;
            _currentUserToken = null; // clear current session

            return await LogoutAsync(token);
        }

        // GET USERNAME FROM TOKEN
        public string? GetUsernameFromToken(string token)
        {
            return _sessions.TryGetValue(token, out var username)
                ? username
                : null;
        }

        // IS TOKEN VALID?
        public bool IsTokenValid(string token)
        {
            return _sessions.ContainsKey(token);
        }

        // GET ACTIVE USER TOKEN
        public string? GetActiveTokenForUser(string username)
        {
            return _userSessions.TryGetValue(username, out var token)
                ? token
                : null;
        }


        // GET CURRENT USERNAME
        public string? GetCurrentUsername()
        {
            if (_currentUserToken == null) return null;
            return GetUsernameFromToken(_currentUserToken);
        }

        // TOKEN GENERATOR
        private static string GenerateToken(string username)
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + username;
        }
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
