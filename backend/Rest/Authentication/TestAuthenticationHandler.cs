using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Rest.Authentication;

/// <summary>
/// Supplies an explicit test principal for REST integration tests.
/// </summary>
internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<TestAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TestAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthenticationDefaults.UserHeader, out Microsoft.Extensions.Primitives.StringValues subject)
            || string.IsNullOrWhiteSpace(subject))
        {
            return Task.FromResult(AuthenticateResult.Fail("A test user is required."));
        }

        Claim[] claims = [new Claim(ClaimTypes.NameIdentifier, subject.ToString()), new Claim("sub", subject.ToString())];
        ClaimsIdentity identity = new(claims, TestAuthenticationDefaults.Scheme, ClaimTypes.Name, ClaimTypes.Role);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), TestAuthenticationDefaults.Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Options for the test-only authentication scheme.
/// </summary>
internal sealed class TestAuthenticationOptions : AuthenticationSchemeOptions;

/// <summary>
/// Constants used by the test-only authentication scheme.
/// </summary>
internal static class TestAuthenticationDefaults
{
    /// <summary>
    /// Authentication scheme name.
    /// </summary>
    internal const string Scheme = "Test";

    /// <summary>
    /// Header containing the test principal's immutable subject.
    /// </summary>
    internal const string UserHeader = "X-Test-User";
}
