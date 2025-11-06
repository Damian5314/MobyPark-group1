using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IUserProfileService
    {
        Task<UserProfile?> GetByUsernameAsync(string username);
        Task<IEnumerable<UserProfile>> GetAllAsync();
        Task<UserProfile> CreateAsync(UserProfile profile);
        Task<UserProfile?> UpdateAsync(string username, UserProfile profile);
        Task<bool> DeleteAsync(string username);
    }
}