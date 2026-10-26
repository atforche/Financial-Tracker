namespace Tests.Support;

/// <summary>
/// Account or fund balance values exposed by the application.
/// </summary>
internal sealed record BalanceSnapshot(decimal Posted, decimal Pending)
{
    /// <summary>
    /// Posted balance plus the net pending amount.
    /// </summary>
    public decimal PostedWithPending => Posted + Pending;
}
