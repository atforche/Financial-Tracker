namespace Rest.Authentication;

/// <summary>
/// Names of the database-backed application authorization policies.
/// </summary>
internal static class UserAuthorizationPolicies
{
    /// <summary>
    /// Policy for the provider identity resolution endpoint.
    /// </summary>
    internal const string ProviderIdentity = "provider-identity";

    /// <summary>
    /// Policy requiring an active application user.
    /// </summary>
    internal const string ActiveUser = "active-user";

    /// <summary>
    /// Policy permitting users who can perform financial writes.
    /// </summary>
    internal const string WriteCapableUser = "write-capable-user";

    /// <summary>
    /// Policy requiring an active administrator.
    /// </summary>
    internal const string Administrator = "administrator";
}
