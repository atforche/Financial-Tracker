using Domain.Funds;

namespace Domain.FundGoals;

/// <summary>
/// Value object describing the current availability of a Fund.
/// </summary>
public sealed class FundAvailability(FundBalance fundBalance)
{
    /// <summary>
    /// Posted balance currently available in the Fund.
    /// </summary>
    public decimal AvailableBalance { get; } = fundBalance.PostedBalance;

    /// <summary>
    /// Balance available after including pending activity.
    /// </summary>
    public decimal AvailableBalanceIncludingPending { get; } = fundBalance.BalanceIncludingPending;

    /// <summary>
    /// True when the posted available balance is below zero.
    /// </summary>
    public bool IsOverspent => AvailableBalance < 0;

    /// <summary>
    /// True when the available balance including pending activity is below zero.
    /// </summary>
    public bool IsOverspentIncludingPending => AvailableBalanceIncludingPending < 0;

}
