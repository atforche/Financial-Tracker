using Models.Accounts;

namespace Models.Funds;

/// <summary>
/// Model representing an Fund Balance
/// </summary>
public class FundBalanceModel
{
    /// <summary>
    /// Fund ID for the Fund Balance
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Balance for the Fund Balance
    /// </summary>
    public required decimal Balance { get; init; }

    /// <summary>
    /// Account Balances for the Fund Balance
    /// </summary>
    public required IReadOnlyCollection<AccountAmountModel> AccountBalances { get; init; } = [];

    /// <summary>
    /// Pending Debit Amount for the Fund Balance
    /// </summary>
    public required decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending Debits for the Fund Balance
    /// </summary>
    public required IReadOnlyCollection<AccountAmountModel> PendingDebits { get; init; } = [];

    /// <summary>
    /// Pending Credit Amount for the Fund Balance
    /// </summary>
    public required decimal PendingCreditAmount { get; init; }

    /// <summary>
    /// Pending Credits for the Fund Balance
    /// </summary>
    public required IReadOnlyCollection<AccountAmountModel> PendingCredits { get; init; } = [];
}