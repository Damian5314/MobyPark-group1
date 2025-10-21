using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public interface IAuthService
    {
        Task<UserProfile?> ValidateUserAsync(string username, string password);
        string HashPassword(string password);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> ValidateUserAsync(string username, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword);

            return user;
        }

        public string HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
