namespace Domain.Funds;

/// <summary>
/// Enum representing the different monthly Fund Goal types.
/// </summary>
public enum FundGoalType
{
    /// <summary>
    /// A monthly fund has a target monthly balance.
    /// </summary>
    Monthly,

    /// <summary>
    /// A rolling fund has a fixed assignment target that carries across months.
    /// </summary>
    Rolling,

    /// <summary>
    /// A savings fund is meant to accumulate toward a future goal.
    /// </summary>
    Savings,

    /// <summary>
    /// A debt fund is used to pay down debt.
    /// </summary>
    Debt,
}