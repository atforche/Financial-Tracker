namespace Tests.Accounts;

/// <summary>
/// Stable reference to an account created through the test context.
/// </summary>
internal sealed record AccountHandle(Guid Id, string Name);
