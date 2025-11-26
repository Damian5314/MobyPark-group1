using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using v2.Services;

namespace v2.Security
{
    public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get required services
            var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var users = context.HttpContext.RequestServices.GetRequiredService<IUserProfileService>();

            // Use the in-memory token (like Logout)
            var currentUsername = auth.GetCurrentUsername();
            Console.WriteLine($"[DEBUG] Current username from memory: {currentUsername}");

            if (currentUsername == null)
            {
                Console.WriteLine("[DEBUG] No active user session.");
                context.Result = new UnauthorizedObjectResult(new { error = "No active user session" });
                return;
            }

            var token = auth.GetActiveTokenForUser(currentUsername);
            Console.WriteLine($"[DEBUG] Active token from memory: {token}");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[DEBUG] No active user token.");
                context.Result = new UnauthorizedObjectResult(new { error = "No active user token" });
                return;
            }

            // Get username from token
            var username = auth.GetUsernameFromToken(token);
            Console.WriteLine($"[DEBUG] Username from token: {username}");

            if (username == null)
            {
                Console.WriteLine("[DEBUG] Invalid token.");
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid token" });
                return;
            }

            // Get user profile
            var user = await users.GetByUsernameAsync(username);
            Console.WriteLine($"[DEBUG] User role: {user?.Role}");

            if (user?.Role != "ADMIN")
            {
                Console.WriteLine("[DEBUG] User is not admin.");
                context.Result = new ObjectResult(new { error = "Admin only" }) { StatusCode = 403 };
                return;
            }

            Console.WriteLine("[DEBUG] Admin access granted.");
            // Allow request to continue
            await next();
        }
    }
}