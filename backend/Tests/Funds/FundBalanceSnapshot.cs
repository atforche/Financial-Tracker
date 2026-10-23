namespace Tests.Funds;

/// <summary>
/// Fund balance values exposed by the application.
/// </summary>
internal sealed record FundBalanceSnapshot(decimal Posted, decimal Pending)
{
    /// <summary>
    /// Posted balance plus the net pending amount.
    /// </summary>
    public decimal PostedWithPending => Posted + Pending;
}
