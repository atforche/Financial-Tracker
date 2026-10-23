namespace Tests.Accounts;

/// <summary>
/// Account balance values exposed by the application.
/// </summary>
internal sealed record AccountBalanceSnapshot(decimal Posted, decimal Pending)
{
    /// <summary>
    /// Posted balance plus the net pending amount.
    /// </summary>
    public decimal PostedWithPending => Posted + Pending;
}
