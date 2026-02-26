using Models.Funds;

namespace Models.Accounts;

/// <summary>
/// Model representing an Account Balance
/// </summary>
public class AccountBalanceModel
{
    /// <summary>
    /// Account ID for the Account Balance
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Posted Balance for the Account Balance
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Available to Spend Balance for the Account Balance
    /// </summary>
    public required decimal? AvailableToSpend { get; init; }

    /// <summary>
    /// Fund Balances for the Account Balance
    /// </summary>
    public required IReadOnlyCollection<FundAmountModel> FundBalances { get; init; } = [];

    /// <summary>
    /// Pending Debit Amount for the Account Balance
    /// </summary>
    public required decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending Debits for the Account Balance
    /// </summary>
    public required IReadOnlyCollection<FundAmountModel> PendingDebits { get; init; } = [];

    /// <summary>
    /// Pending Credit Amount for the Account Balance
    /// </summary>
    public required decimal PendingCreditAmount { get; init; }

    /// <summary>
    /// Pending Credits for the Account Balance
    /// </summary>
    public required IReadOnlyCollection<FundAmountModel> PendingCredits { get; init; } = [];
}