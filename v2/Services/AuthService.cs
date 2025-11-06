using v2.Models;
using System.Security.Cryptography;
using System.Text;

namespace v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly List<UserProfile> _users = new(); // mock storage
        private readonly Dictionary<string, string> _sessions = new();

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (_users.Any(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists.");

            var hashed = HashPassword(request.Password);

            var user = new UserProfile
            {
                Id = _users.Count + 1,
                Username = request.Username,
                Password = hashed,
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                Active = true,
                Role = "USER"
            };

            _users.Add(user);

            var token = GenerateToken(user.Username);
            _sessions[token] = user.Username;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || !VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            var token = GenerateToken(user.Username);
            _sessions[token] = user.Username;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        // Helper methods
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string input, string storedHash)
            => HashPassword(input) == storedHash;

        private static string GenerateToken(string username)
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + username;
    }
}