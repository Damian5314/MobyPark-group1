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

        // UPDATE USER (self-service: allow Name + Username only, no role changes)
        public async Task<UserProfile?> UpdateAsync(string username, UpdateMyProfileDto dto)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return null;

            // ---- Username change (optional) ----
            var requestedUsername = dto.Username?.Trim();

            if (!string.IsNullOrWhiteSpace(requestedUsername) &&
                !string.Equals(existing.Username, requestedUsername, StringComparison.OrdinalIgnoreCase))
            {
                // Check uniqueness
                var taken = await _db.Users.AnyAsync(u => u.Username == requestedUsername);
                if (taken)
                    throw new InvalidOperationException("Username already taken.");

                existing.Username = requestedUsername;
            }

            // ---- Name change ----
            existing.Name = dto.Name;

            // Keep these if you still want them editable too
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.BirthYear = dto.BirthYear;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return false;

            // verify current password
            if (!PasswordHelper.VerifyPassword(currentPassword, existing.Password))
                return false;

            // hash new password
            var newHash = PasswordHelper.HashPassword(newPassword);

            // if the new password is same as old (or user typed same), nothing changes
            if (existing.Password == newHash)
                return true;

            existing.Password = newHash;

            // force EF to treat it as modified (helpful if Password isn't mapped correctly)
            _db.Entry(existing).Property(u => u.Password).IsModified = true;

            var written = await _db.SaveChangesAsync();

            // If nothing was written, it means EF didn't persist anything (mapping / db issue)
            if (written <= 0) return false;

            // Re-load from DB to confirm it really changed
            var reloaded = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == existing.Username);

            if (reloaded == null) return false;

            // Confirm DB now matches new password
            return PasswordHelper.VerifyPassword(newPassword, reloaded.Password);
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

    // DTO defined in same file
    public class UpdateMyProfileDto
    {
        public string Username { get; set; } = ""; // NEW
        public string Name { get; set; } = "";

        // keep these only if you still want user to edit them
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public int? BirthYear { get; set; }
    }
}
