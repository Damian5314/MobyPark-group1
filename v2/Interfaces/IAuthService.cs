using v2.Models;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(string token);

        // NEW METHODS
        Task<bool> LogoutCurrentUserAsync();
        string? GetCurrentUsername();

        string? GetUsernameFromToken(string token);
        bool IsTokenValid(string token);
        string? GetActiveTokenForUser(string username);
    }
}
