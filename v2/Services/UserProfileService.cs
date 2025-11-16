using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Security;

namespace v2.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _db;

        public UserProfileService(AppDbContext db)
        {
            _db = db;
        }

        // GET USER BY USERNAME
        public async Task<UserProfile?> GetByUsernameAsync(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        // GET USER BY ID
        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        // GET ALL USERS
        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        // UPDATE USER
        public async Task<UserProfile?> UpdateAsync(string username, UserProfile profile)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return null;

            existing.Name = profile.Name;
            existing.Email = profile.Email;
            existing.Phone = profile.Phone;
            existing.Role = profile.Role;
            existing.BirthYear = profile.BirthYear;
            existing.Active = profile.Active;

            await _db.SaveChangesAsync();
            return existing;
        }

        // CHANGE PASSWORD (USER)
        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return false;

            if (!PasswordHelper.VerifyPassword(currentPassword, existing.Password))
                return false;

            existing.Password = PasswordHelper.HashPassword(newPassword);
            await _db.SaveChangesAsync();

            return true;
        }

        // SET PASSWORD (ADMIN)
        public async Task<bool> SetPasswordAsync(string username, string newPassword)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return false;

            existing.Password = PasswordHelper.HashPassword(newPassword);
            await _db.SaveChangesAsync();

            return true;
        }

        // DELETE USER
        public async Task<bool> DeleteAsync(string username)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
