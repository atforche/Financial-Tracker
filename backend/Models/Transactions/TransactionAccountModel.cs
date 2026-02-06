using Models.Accounts;
using Models.Funds;

namespace Models.Transactions;

/// <summary>
/// Model representing a Transaction Account
/// </summary>
public class TransactionAccountModel
{
    /// <summary>
    /// Account ID for the Transaction Account
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Account Name for the Transaction Account
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Posted Date for the Transaction Account
    /// </summary>
    public required DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund Amounts for the Transaction Account
    /// </summary>
    public required IReadOnlyCollection<FundAmountModel> FundAmounts { get; init; }

    /// <summary>
    /// Previous Account Balance for the Transaction Account
    /// </summary>
    public required AccountBalanceModel PreviousAccountBalance { get; init; }

    /// <summary>
    /// New Account Balance for the Transaction Account
    /// </summary>
    public required AccountBalanceModel NewAccountBalance { get; init; }
}