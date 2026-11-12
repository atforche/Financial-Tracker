using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Locations;

namespace Domain.Transactions.BalanceAdjustments;

/// <summary>
/// Transaction that changes the balance of a single untracked Account without a counterparty, such as a change in
/// market value or accrued interest.
/// </summary>
/// <remarks>
/// The Amount is a signed change to the balance as it is displayed, so a positive Amount on a Debt Account means more
/// is owed. A Balance Adjustment is always posted on its date.
/// </remarks>
public class BalanceAdjustment : Transaction
{
    /// <summary>
    /// Account whose balance is adjusted
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Gets the balance event type that this adjustment represents for its Account.
    /// </summary>
    /// <remarks>
    /// Balance events describe debits and credits, so an increase in the displayed balance is a credit for most Accounts
    /// and a debit for Debt Accounts.
    /// </remarks>
    public BalanceEventType BalanceEventType => (Amount > 0) != Account.Type.IsDebt()
        ? BalanceEventType.Credit
        : BalanceEventType.Debit;

    /// <summary>
    /// Gets the unsigned amount of the balance event that this adjustment represents for its Account.
    /// </summary>
    public decimal BalanceEventAmount => Math.Abs(Amount);

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [Account.Id];

    /// <inheritdoc/>
    public override IEnumerable<LocationId> GetAllAffectedLocationIds() => [];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => accountId == Account.Id ? Date : null;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => [];

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BalanceAdjustment(CreateBalanceAdjustmentRequest request, int sequence)
        : base(request, sequence, TransactionType.BalanceAdjustment)
    {
        Account = request.Account;
    }

    /// <summary>
    /// Updates the Account whose balance is adjusted
    /// </summary>
    internal void UpdateAccount(Account account) => Account = account;

    /// <inheritdoc/>
    /// <remarks>
    /// A Balance Adjustment is always posted on its date, so there is no separate posted date to set.
    /// </remarks>
    internal override void SetPostedDate(AccountId accountId, DateOnly? postedDate) =>
        throw new InvalidOperationException("Balance adjustments are always posted on their date.");

    /// <inheritdoc/>
    internal override void ClearPostedDates()
    {
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected BalanceAdjustment()
        : base()
    {
        Account = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse) =>
        existingAccountBalance.Account.Id == Account.Id
            ? existingAccountBalance.Adjust(reverse ? -Amount : Amount)
            : existingAccountBalance;

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override FundGoalTotals AddToFundGoalTotals(FundGoalTotals existingTotals, bool reverse) => existingTotals;

    /// <inheritdoc/>
    protected override FundGoalTotals PostToFundGoalTotals(FundGoalTotals existingTotals, AccountId accountId, bool reverse) => existingTotals;

    /// <inheritdoc/>
    internal override void ReplaceLocation(Location source, Location target)
    {
    }
}
