using Domain.Accounts;
using Domain.Funds;
using Domain.Goals;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Entity class representing an account transaction.
/// </summary>
/// <remarks>
/// An account transaction represents one of the following scenarios:
///     1. A transaction that only debits an untracked account
///     2. A transaction that only credits an untracked account
///     3. A transaction that moves money between two untracked accounts
///     4. A transaction that moves money between two tracked accounts
/// </remarks>
public class AccountTransaction : Transaction
{
    private readonly List<AccountTransactionDestination> _destinations = [];

    /// <summary>
    /// Source for this Account Transaction
    /// </summary>
    public AccountTransactionSource Source { get; private set; }

    /// <summary>
    /// Destinations for this Account Transaction
    /// </summary>
    public IReadOnlyList<AccountTransactionDestination> Destinations => _destinations;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        if (Source.Account != null)
        {
            yield return Source.Account.Id;
        }
        foreach (AccountId? accountId in Destinations.Select(d => d.Account?.Id))
        {
            if (accountId != null)
            {
                yield return accountId;
            }
        }
    }

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (Source.Account != null && accountId == Source.Account.Id)
        {
            return Source.PostedDate;
        }
        AccountTransactionDestination? destination = Destinations.FirstOrDefault(d => d.Account != null && d.Account.Id == accountId);
        if (destination != null)
        {
            return destination.PostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => [];

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountTransaction(CreateAccountTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Account)
    {
        Source = request.Source;
        UpdateAccountDestinations(request.Destinations);
    }

    /// <summary>
    /// Updates the account transaction source for this account transaction.
    /// </summary>
    internal void UpdateAccountSource(AccountTransactionSource source) => Source = source;

    /// <summary>
    /// Updates the account transaction destinations for this account transaction.
    /// </summary>
    internal void UpdateAccountDestinations(IReadOnlyCollection<AccountTransactionDestination> destinations)
    {
        _destinations.Clear();
        _destinations.AddRange(destinations);
    }

    /// <summary>
    /// Sets the posted date for a specific account affected by this transaction.
    /// </summary>
    internal void SetPostedDate(AccountId accountId, DateOnly? postedDate)
    {
        if (accountId == Source.Account?.Id)
        {
            Source.PostedDate = postedDate;
            return;
        }
        AccountTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == accountId);
        _ = (destination?.PostedDate = postedDate);
    }

    /// <summary>
    /// Clears all posted dates for this transaction.
    /// </summary>
    internal void ClearPostedDates()
    {
        Source.PostedDate = null;
        foreach (AccountTransactionDestination destination in _destinations)
        {
            destination.PostedDate = null;
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected AccountTransaction()
        : base()
    {
        Source = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        AccountTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == existingAccountBalance.Account.Id);
        if (destination != null)
        {
            return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
        }
        if (existingAccountBalance.Account.Id == Source.Account?.Id)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        AccountTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == existingAccountBalance.Account.Id);
        if (destination != null)
        {
            return existingAccountBalance.PostPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
        }
        if (existingAccountBalance.Account.Id == Source.Account?.Id)
        {
            return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override GoalBalance AddToGoalBalance(GoalBalance existingGoalBalance, bool reverse) => existingGoalBalance;

    /// <inheritdoc/>
    protected override GoalBalance PostToGoalBalance(GoalBalance existingGoalBalance, AccountId accountId, bool reverse) => existingGoalBalance;
}