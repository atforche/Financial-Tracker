using System.Net;
using Tests.Infrastructure;

namespace Tests.Authentication;

/// <summary>
/// Verifies that API endpoints reject requests without an authenticated principal.
/// </summary>
public sealed class AuthenticationEndpointTests
{
    /// <summary>
    /// Rejects an anonymous request before it can access financial data.
    /// </summary>
    [Fact]
    public async Task AnonymousRequestIsUnauthorized()
    {
        using FinancialTrackerApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
