using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserProfileService
{
    Task<UserProfile?> GetByUsernameAsync(string username);
    Task<UserProfile?> GetByIdAsync(int id);
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task<UserProfile?> UpdateAsync(string username, UserProfile profile);

    // NEW:
    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword); // user changing own password
    Task<bool> SetPasswordAsync(string username, string newPassword); // admin reset
    Task<bool> DeleteAsync(string username);
}
