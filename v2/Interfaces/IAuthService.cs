using v2.Models;

namespace v2.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string token);

        string? GetUsernameFromToken(string token);   // ADD THIS
        bool IsTokenValid(string token);              // OPTIONAL BUT RECOMMENDED
    }

}