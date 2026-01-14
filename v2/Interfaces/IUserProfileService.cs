using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using v2.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetByUsernameAsync(string username);
    Task<UserProfile?> GetByIdAsync(int id);
    Task<IEnumerable<UserProfile>> GetAllAsync();

    // FIXED: use DTO, not entity
    Task<UserProfile?> UpdateAsync(string username, UpdateMyProfileDto dto);

    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    Task<bool> SetPasswordAsync(string username, string newPassword);
    Task<bool> DeleteAsync(string username);
    Task<UserProfile?> AdminUpdateAsync(string targetUsername, AdminUpdateUserDto dto);
}
