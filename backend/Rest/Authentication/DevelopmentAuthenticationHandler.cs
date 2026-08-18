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
        string subject = token.StartsWith("development:", StringComparison.Ordinal)
            ? token["development:".Length..]
            : "";
        string additionalSubjects = Environment.GetEnvironmentVariable(DevelopmentAuthenticationDefaults.AdditionalSubjectsEnvironmentVariable)
            ?? (string.Equals(expectedSubject, "local-developer", StringComparison.Ordinal)
                ? "local-standard,local-read-only"
                : "");
        string[] allowedSubjects =
        [
            expectedSubject,
            ..additionalSubjects
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        ];
        if (!allowedSubjects.Contains(subject, StringComparer.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("The development access token is invalid."));
        }

        string email = subject == expectedSubject
            ? Environment.GetEnvironmentVariable(DevelopmentAuthenticationDefaults.EmailEnvironmentVariable)
                ?? "local-developer@example.test"
            : $"{subject}@example.test";
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim("sub", subject),
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
    /// Optional comma-separated additional local identities accepted by guarded development authentication.
    /// </summary>
    internal const string AdditionalSubjectsEnvironmentVariable = "DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS";

    /// <summary>
    /// Environment variable containing the local developer email claim.
    /// </summary>
    internal const string EmailEnvironmentVariable = "DEVELOPMENT_AUTH_EMAIL";
}
