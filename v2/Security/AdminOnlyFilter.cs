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

            string token = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (!token.StartsWith("Bearer "))
            {
                // fallback to current logged-in user's token from memory
                var currentUsername = auth.GetCurrentUsername();
                if (currentUsername == null)
                {
                    context.Result = new UnauthorizedObjectResult(new { error = "Missing token and no active user" });
                    return;
                }

                token = auth.GetActiveTokenForUser(currentUsername) ?? "";
            }
            else
            {
                // Remove "Bearer " prefix
                token = token.Substring("Bearer ".Length).Trim();
            }

            // Get username from token
            var username = auth.GetUsernameFromToken(token);
            if (username == null)
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid token" });
                return;
            }

            // Get user profile
            var user = await users.GetByUsernameAsync(username);
            if (user?.Role != "ADMIN")
            {
                context.Result = new ObjectResult(new { error = "Admin only" }) { StatusCode = 403 };
                return;
            }

            // Allow request to continue
            await next();
        }
    }
}