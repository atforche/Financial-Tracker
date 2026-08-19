using System.Net;
using Tests.Infrastructure;

namespace Tests.Authentication;

/// <summary>
/// Verifies the production JWT validation and database-backed subject authorization contract.
/// </summary>
public sealed class JwtBearerAuthenticationTests
{
    /// <summary>
    /// Allows a valid token when its subject has an active application user.
    /// </summary>
    [Fact]
    public async Task ProvisionedSubjectCanAccessProtectedEndpoint()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken(JwtBearerAuthenticationApplicationFactory.ProvisionedSubject));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Rejects a genuine token for a subject without an application user.
    /// </summary>
    [Fact]
    public async Task UnprovisionedSubjectIsForbidden()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken("unapproved-test-subject"));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Rejects tokens expired beyond the configured clock-skew allowance.
    /// </summary>
    [Fact]
    public async Task ExpiredTokenIsUnauthorized()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken(
            JwtBearerAuthenticationApplicationFactory.ProvisionedSubject,
            expires: DateTime.UtcNow.AddMinutes(-6)));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Rejects tokens minted for another Google OAuth client.
    /// </summary>
    [Fact]
    public async Task WrongAudienceTokenIsUnauthorized()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken(
            JwtBearerAuthenticationApplicationFactory.ProvisionedSubject,
            audience: "other-client"));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Rejects tokens not issued by the configured OpenID Connect issuer.
    /// </summary>
    [Fact]
    public async Task WrongIssuerTokenIsUnauthorized()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken(
            JwtBearerAuthenticationApplicationFactory.ProvisionedSubject,
            issuer: "https://other-issuer.test"));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
