using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Rest.Authentication;

/// <summary>
/// Authenticates the explicit local developer identity used only by the Development environment.
/// </summary>
internal sealed class DevelopmentAuthenticationHandler(
    IOptionsMonitor<DevelopmentAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<DevelopmentAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string bearerPrefix = "Bearer ";
        if (!Request.Headers.Authorization.ToString().StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("A development access token is required."));
        }

        string expectedSubject = Environment.GetEnvironmentVariable(DevelopmentAuthenticationDefaults.SubjectEnvironmentVariable) ?? "";
        if (string.IsNullOrWhiteSpace(expectedSubject))
        {
            return Task.FromResult(AuthenticateResult.Fail("A development subject is required."));
        }

        string token = Request.Headers.Authorization.ToString()[bearerPrefix.Length..];
        if (!string.Equals(token, $"development:{expectedSubject}", StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("The development access token is invalid."));
        }

        string email = Environment.GetEnvironmentVariable(DevelopmentAuthenticationDefaults.EmailEnvironmentVariable)
            ?? "local-developer@example.test";
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, expectedSubject),
            new Claim("sub", expectedSubject),
            new Claim("email", email),
            new Claim("email_verified", "true"),
            new Claim("name", "Local developer"),
        ];
        ClaimsIdentity identity = new(claims, DevelopmentAuthenticationDefaults.Scheme, ClaimTypes.Name, ClaimTypes.Role);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), DevelopmentAuthenticationDefaults.Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Options for the Development-only authentication scheme.
/// </summary>
internal sealed class DevelopmentAuthenticationOptions : AuthenticationSchemeOptions;

/// <summary>
/// Constants used by the Development-only authentication scheme.
/// </summary>
internal static class DevelopmentAuthenticationDefaults
{
    /// <summary>
    /// Authentication scheme name.
    /// </summary>
    internal const string Scheme = "Development";

    /// <summary>
    /// Environment variable containing the local developer subject.
    /// </summary>
    internal const string SubjectEnvironmentVariable = "DEVELOPMENT_AUTH_SUBJECT";

    /// <summary>
    /// Environment variable containing the local developer email claim.
    /// </summary>
    internal const string EmailEnvironmentVariable = "DEVELOPMENT_AUTH_EMAIL";
}