using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Class that represents the filters that can be applied when retrieving Transactions
/// </summary>
public class TransactionFilter
{
    /// <summary>
    /// Account ID
    /// </summary>
    public AccountId? AccountId { get; init; }

    /// <summary>
    /// Accounting Period ID
    /// </summary>
    public AccountingPeriodId? AccountingPeriodId { get; init; }

    /// <summary>
    /// Fund ID
    /// </summary>
    public FundId? FundId { get; init; }
}