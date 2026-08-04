using System.Net;
using Tests.Infrastructure;

namespace Tests.Authentication;

/// <summary>
/// Verifies the production JWT validation and subject authorization contract.
/// </summary>
public sealed class JwtBearerAuthenticationTests
{
    /// <summary>
    /// Allows a valid token only when its literal Google subject is allowlisted.
    /// </summary>
    [Fact]
    public async Task AllowlistedSubjectCanAccessProtectedEndpoint()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.CreateToken(JwtBearerAuthenticationApplicationFactory.AllowedSubject));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Rejects a genuine token for a subject that is not an approved collaborator.
    /// </summary>
    [Fact]
    public async Task UnallowlistedSubjectIsForbidden()
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
            JwtBearerAuthenticationApplicationFactory.AllowedSubject,
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
            JwtBearerAuthenticationApplicationFactory.AllowedSubject,
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
            JwtBearerAuthenticationApplicationFactory.AllowedSubject,
            issuer: "https://other-issuer.test"));

        using HttpResponseMessage response = await client.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}