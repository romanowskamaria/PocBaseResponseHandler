namespace PocBaseResponseHandler.Tests.Factories;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IList<Claim> claims;

    public TestAuthHandler(TestClaimsProvider claimsProvider, ISystemClock clock, UrlEncoder encoder, ILoggerFactory logger, IOptionsMonitor<AuthenticationSchemeOptions> options) : base(options, logger, encoder, clock) 
        => claims = claimsProvider.Claims;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
