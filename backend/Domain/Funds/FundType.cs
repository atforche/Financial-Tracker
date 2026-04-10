namespace Domain.Funds;

/// <summary>
/// Enum representing the different Fund types
/// </summary>
public enum FundType
{
    /// <summary>
    /// Unassigned Fund
    /// </summary>
    /// <remarks>
    /// The unassigned fund is a special system defined fund that is used to hold any unassigned money. 
    /// The unassigned fund is not meant to be used by the user and should not be assigned to any transactions.
    /// </remarks>
    Unassigned,

    /// <summary>
    /// Monthly Fund
    /// </summary>
    /// <remarks>
    /// A monthly fund is a fund that has a target monthly balance. The assignment target for a monthly fund is the 
    /// difference between the previous month's balance and the target monthly balance.
    /// </remarks>
    Monthly,

    /// <summary>
    /// Rolling Fund
    /// </summary>
    /// <remarks>
    /// A rolling fund is a fund where the assignment target is fixed and does not fluctuate based on the previous month's balance. 
    /// </remarks>
    Rolling,

    /// <summary>
    /// Savings Fund
    /// </summary>
    /// <remarks>
    /// A savings fund is a fund that is meant to be saved up over time for a specific goal.
    /// </remarks>
    Savings,

    /// <summary>
    /// Debt Fund
    /// </summary>
    /// <remarks>
    /// A debt fund is a fund that is used to pay off debts.
    /// </remarks>
    Debt,
}