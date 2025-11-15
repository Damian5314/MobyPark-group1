using v2.Models;
using v2.Security;


namespace v2.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly List<UserProfile> _users = new();

        public async Task<UserProfile?> GetByUsernameAsync(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return _users;
        }

        public async Task<UserProfile?> UpdateAsync(string username, UserProfile profile)
        {
            var existing = _users.FirstOrDefault(u => u.Username == username);
            if (existing == null) return null;

            existing.Name = profile.Name;
            existing.Email = profile.Email;
            existing.Phone = profile.Phone;
            existing.Role = profile.Role;
            existing.BirthYear = profile.BirthYear;
            existing.Active = profile.Active;


            return existing;
        }

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var existing = _users.FirstOrDefault(u => u.Username == username);
            if (existing == null) return false;

            // Check current password
            if (!PasswordHelper.VerifyPassword(currentPassword, existing.Password))
            {
                return false;
            }

            // Set new hashed password
            existing.Password = PasswordHelper.HashPassword(newPassword);
            return true;
        }

        public async Task<bool> SetPasswordAsync(string username, string newPassword)
        {
            var existing = _users.FirstOrDefault(u => u.Username == username);
            if (existing == null) return false;

            existing.Password = PasswordHelper.HashPassword(newPassword);
            return true;
        }

        public async Task<bool> DeleteAsync(string username)
        {
            var existing = _users.FirstOrDefault(u => u.Username == username);
            if (existing == null) return false;

            _users.Remove(existing);
            return true;
        }
    }
}
