using System.Security.Claims;
using Domain.Users;

namespace Rest.Authentication;

/// <summary>
/// Validated identity claims supplied by the configured OpenID Connect provider.
/// </summary>
internal readonly record struct ProviderIdentity(
    string Subject,
    string Email,
    bool EmailVerified,
    string? DisplayName);

/// <summary>
/// Reads and validates the provider claims required for first-login resolution.
/// </summary>
internal static class ProviderIdentityClaims
{
    /// <summary>
    /// Attempts to read a valid provider identity from an authenticated principal.
    /// </summary>
    internal static bool TryRead(ClaimsPrincipal principal, out ProviderIdentity identity)
    {
        identity = default;
        string? subject = principal.FindFirst("sub")?.Value?.Trim();
        string? email = principal.FindFirst("email")?.Value;
        string? emailVerified = principal.FindFirst("email_verified")?.Value;
        string? displayName = principal.FindFirst("name")?.Value;
        if (string.IsNullOrWhiteSpace(subject)
            || subject.Length > 255
            || !bool.TryParse(emailVerified, out bool isEmailVerified)
            || !isEmailVerified
            || !UserEmail.TryNormalize(email, out _, out _)
            || displayName?.Length > 255)
        {
            return false;
        }

        identity = new ProviderIdentity(subject, email!, true, displayName);
        return true;
    }
}
