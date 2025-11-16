using v2.Models;

namespace v2.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(string token);
        
        string? GetUsernameFromToken(string token);
        bool IsTokenValid(string token);
        string? GetActiveTokenForUser(string username);
    }
}
