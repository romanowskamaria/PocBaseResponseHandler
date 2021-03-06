namespace PocBaseResponseHandler.Handlers;

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(ISystemClock clock, UrlEncoder encoder, ILoggerFactory logger, IOptionsMonitor<AuthenticationSchemeOptions> options)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.Fail("Missing"));
    }
}
