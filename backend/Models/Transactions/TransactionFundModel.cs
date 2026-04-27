using Models.Funds;

namespace Models.Transactions;

/// <summary>
/// Model representing a fund referenced by a transaction.
/// </summary>
public class TransactionFundModel
{
    /// <summary>
    /// Fund ID.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name.
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Amount assigned to the fund for the transaction.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Previous Fund Balance for the Transaction Fund
    /// </summary>
    public required FundBalanceModel PreviousFundBalance { get; init; }

    /// <summary>
    /// New Fund Balance for the Transaction Fund
    /// </summary>
    public required FundBalanceModel NewFundBalance { get; init; }
}