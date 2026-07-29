namespace Tests.Transactions;

/// <summary>
/// Stable reference to a transaction created through the test context.
/// </summary>
internal sealed record TransactionHandle(Guid Id);