namespace PocBaseResponseHandler.Tests.Factories;

using System.Security.Claims;

public class TestClaimsProvider
{
    public IList<Claim> Claims { get; } = new List<Claim>();

    public static TestClaimsProvider WithAdminClaims()
    {
        var provider = new TestClaimsProvider();
        provider.Claims.Add(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        provider.Claims.Add(new Claim(ClaimTypes.Name, "Admin user"));
        provider.Claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        return provider;
    }

    public static TestClaimsProvider WithUserClaims()
    {
        var provider = new TestClaimsProvider();
        provider.Claims.Add(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        provider.Claims.Add(new Claim(ClaimTypes.Name, "User"));

        return provider;
    }
}
