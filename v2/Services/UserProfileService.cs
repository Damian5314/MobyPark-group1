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

            // Username change (optional)
            var requestedUsername = dto.Username?.Trim();

            if (!string.IsNullOrWhiteSpace(requestedUsername) &&
                !string.Equals(existing.Username, requestedUsername, StringComparison.OrdinalIgnoreCase))
            {
                var taken = await _db.Users.AnyAsync(u => u.Username == requestedUsername);
                if (taken)
                    throw new InvalidOperationException("Username already taken.");

                existing.Username = requestedUsername;
            }

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.BirthYear = dto.BirthYear;

            await _db.SaveChangesAsync();
            return existing;
        }

        // ✅ ADMIN UPDATE USER BY USERNAME (can change role/active + profile fields + username)
        public async Task<UserProfile?> AdminUpdateAsync(string targetUsername, AdminUpdateUserDto dto)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);
            if (existing == null) return null;

            // Optional username change
            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var newUsername = dto.Username.Trim();

                if (!string.Equals(existing.Username, newUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var taken = await _db.Users.AnyAsync(u => u.Username == newUsername);
                    if (taken)
                        throw new InvalidOperationException("Username already taken.");

                    existing.Username = newUsername;
                }
            }

            // Update fields if provided (partial updates for admin)
            if (dto.Name != null) existing.Name = dto.Name;
            if (dto.Email != null) existing.Email = dto.Email;
            if (dto.Phone != null) existing.Phone = dto.Phone;
            if (dto.BirthYear.HasValue) existing.BirthYear = dto.BirthYear.Value;

            if (dto.Role != null) existing.Role = dto.Role;
            if (dto.Active.HasValue) existing.Active = dto.Active.Value;

            // ✅ Password update if provided
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                existing.Password = PasswordHelper.HashPassword(dto.NewPassword);

            await _db.SaveChangesAsync();
            return existing;
        }


        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing == null) return false;

            if (!PasswordHelper.VerifyPassword(currentPassword, existing.Password))
                return false;

            var newHash = PasswordHelper.HashPassword(newPassword);

            if (existing.Password == newHash)
                return true;

            existing.Password = newHash;
            _db.Entry(existing).Property(u => u.Password).IsModified = true;

            var written = await _db.SaveChangesAsync();
            if (written <= 0) return false;

            var reloaded = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == existing.Username);

            return reloaded != null && PasswordHelper.VerifyPassword(newPassword, reloaded.Password);
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

    // Self-service DTO
    public class UpdateMyProfileDto
    {
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public int? BirthYear { get; set; }
    }

    // ✅ Admin DTO (nullable fields = partial updates)
    public class AdminUpdateUserDto
    {
        public string? Username { get; set; }   // optional rename
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? BirthYear { get; set; }

        public string? Role { get; set; }
        public bool? Active { get; set; }

        public string? NewPassword { get; set; } // ✅ NEW: admin can set password
    }
}
