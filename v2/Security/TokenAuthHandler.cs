using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using v2.Services;

public class TokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;

    public TokenAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAuthService authService)
        : base(options, logger, encoder, clock)
    {
        _authService = authService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var header = Request.Headers["Authorization"].ToString();

        if (!header.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));

        var token = header.Replace("Bearer ", "");

        // Step 1: token must exist
        if (!_authService.IsTokenValid(token))
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token."));

        // Step 2: get username
        var username = _authService.GetUsernameFromToken(token);
        if (username == null)
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token."));

        // Step 3: token must match user's active token
        var activeToken = _authService.GetActiveTokenForUser(username);
        if (activeToken != token)
            return Task.FromResult(AuthenticateResult.Fail("Expired session or logged out."));

        // Build claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "USER") // Later you can load actual role
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
