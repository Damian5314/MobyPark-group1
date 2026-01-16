using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Security;

namespace v2.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(AppDbContext db, ILogger<UserProfileService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET USER BY USERNAME
        public async Task<UserProfile?> GetByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching user profile for username: {Username}", username);
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User with username {Username} not found", username);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for username: {Username}", username);
                throw;
            }
        }

        // GET USER BY ID
        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching user profile for ID: {UserId}", id);
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for ID: {UserId}", id);
                throw;
            }
        }

        // GET ALL USERS
        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all user profiles");
                var users = await _db.Users.ToListAsync();
                _logger.LogInformation("Retrieved {UserCount} user profiles", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all user profiles");
                throw;
            }
        }

        // UPDATE USER (self-service: allow Name + Username only, no role changes)
        public async Task<UserProfile?> UpdateAsync(string username, UpdateMyProfileDto dto)
        {
            try
            {
                _logger.LogInformation("Updating user profile for username: {Username}", username);

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (existing == null)
                {
                    _logger.LogWarning("User with username {Username} not found for update", username);
                    return null;
                }

                // Username change (optional)
                var requestedUsername = dto.Username?.Trim();

                if (!string.IsNullOrWhiteSpace(requestedUsername) &&
                    !string.Equals(existing.Username, requestedUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var taken = await _db.Users.AnyAsync(u => u.Username == requestedUsername);
                    if (taken)
                    {
                        _logger.LogWarning("Username {RequestedUsername} already taken", requestedUsername);
                        throw new InvalidOperationException("Username already taken.");
                    }

                    existing.Username = requestedUsername;
                }

                existing.Name = dto.Name;
                existing.Email = dto.Email;
                existing.Phone = dto.Phone;
                existing.BirthYear = dto.BirthYear;

                await _db.SaveChangesAsync();

                _logger.LogInformation("User profile {Username} updated successfully", username);

                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for username: {Username}", username);
                throw;
            }
        }

        // ✅ ADMIN UPDATE USER BY USERNAME (can change role/active + profile fields + username)
        public async Task<UserProfile?> AdminUpdateAsync(string targetUsername, AdminUpdateUserDto dto)
        {
            try
            {
                _logger.LogInformation("Admin updating user profile for username: {Username}", targetUsername);

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);
                if (existing == null)
                {
                    _logger.LogWarning("User with username {Username} not found for admin update", targetUsername);
                    return null;
                }

                // Optional username change
                if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var newUsername = dto.Username.Trim();

                    if (!string.Equals(existing.Username, newUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        var taken = await _db.Users.AnyAsync(u => u.Username == newUsername);
                        if (taken)
                        {
                            _logger.LogWarning("Admin update failed: Username {NewUsername} already taken", newUsername);
                            throw new InvalidOperationException("Username already taken.");
                        }

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
                {
                    existing.Password = PasswordHelper.HashPassword(dto.NewPassword);
                    _logger.LogWarning("Admin reset password for user: {Username}", targetUsername);
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation("Admin update successful for user: {Username}", targetUsername);

                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin update for username: {Username}", targetUsername);
                throw;
            }
        }


        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            try
            {
                _logger.LogInformation("Password change attempt for username: {Username}", username);

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (existing == null)
                {
                    _logger.LogWarning("User with username {Username} not found for password change", username);
                    return false;
                }

                if (!PasswordHelper.VerifyPassword(currentPassword, existing.Password))
                {
                    _logger.LogWarning("Invalid current password for username: {Username}", username);
                    return false;
                }

                var newHash = PasswordHelper.HashPassword(newPassword);

                if (existing.Password == newHash)
                    return true;

                existing.Password = newHash;
                _db.Entry(existing).Property(u => u.Password).IsModified = true;

                var written = await _db.SaveChangesAsync();
                if (written <= 0)
                {
                    _logger.LogWarning("Password change failed to save for username: {Username}", username);
                    return false;
                }

                var reloaded = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == existing.Username);

                bool success = reloaded != null && PasswordHelper.VerifyPassword(newPassword, reloaded.Password);

                if (success)
                {
                    _logger.LogInformation("Password changed successfully for username: {Username}", username);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for username: {Username}", username);
                throw;
            }
        }

        // SET PASSWORD (ADMIN)
        public async Task<bool> SetPasswordAsync(string username, string newPassword)
        {
            try
            {
                _logger.LogInformation("Admin password reset for username: {Username}", username);

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (existing == null)
                {
                    _logger.LogWarning("User with username {Username} not found for admin password reset", username);
                    return false;
                }

                existing.Password = PasswordHelper.HashPassword(newPassword);
                await _db.SaveChangesAsync();

                _logger.LogWarning("Admin password reset successful for username: {Username}", username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting password for username: {Username}", username);
                throw;
            }
        }

        // DELETE USER
        public async Task<bool> DeleteAsync(string username)
        {
            try
            {
                _logger.LogInformation("Attempting to delete user: {Username}", username);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User with username {Username} not found for deletion", username);
                    return false;
                }

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                _logger.LogWarning("User {Username} deleted successfully", username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Username}", username);
                throw;
            }
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
