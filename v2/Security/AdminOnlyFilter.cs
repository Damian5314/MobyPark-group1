using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using v2.Services;

namespace v2.Security
{
    public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var users = context.HttpContext.RequestServices.GetRequiredService<IUserProfileService>();

            var header = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (!header.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Missing token" });
                return;
            }

            var token = header.Substring("Bearer ".Length).Trim();
            var username = auth.GetUsernameFromToken(token);

            if (username == null)
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid token" });
                return;
            }


            await next();
        }
    }
}