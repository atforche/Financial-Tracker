namespace Tests.FundGoals;

/// <summary>
/// Fund goal availability values exposed by the application.
/// </summary>
internal sealed record FundGoalAvailabilitySnapshot(decimal Posted, decimal Pending)
{
    /// <summary>
    /// Posted balance plus the net pending amount.
    /// </summary>
    public decimal PostedWithPending => Posted + Pending;
}
