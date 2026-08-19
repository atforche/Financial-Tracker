using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Locations;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Entity class representing an account transaction.
/// </summary>
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
    public override IEnumerable<LocationId> GetAllAffectedLocationIds()
    {
        if (Source.Location != null)
        {
            yield return Source.Location.Id;
        }
        foreach (LocationId locationId in Destinations
            .Where(destination => destination.Location != null)
            .Select(destination => destination.Location!.Id)
            .Distinct())
        {
            yield return locationId;
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
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        AccountTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == existingAccountBalance.Account.Id);
        if (destination != null)
        {
            return existingAccountBalance.Credit(reverse ? -destination.Amount : destination.Amount);
        }
        if (existingAccountBalance.Account.Id == Source.Account?.Id)
        {
            return existingAccountBalance.Debit(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

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
        if (Source.Location == source)
        {
            Source = new AccountTransactionSource(Source.Account, Source.PostedDate, target);
        }
        var destinations = _destinations
            .Select(destination => destination.Location == source
                ? new AccountTransactionDestination(destination.Account, destination.PostedDate, target, destination.Amount)
                : destination)
            .ToList();
        var targetDestinations = destinations
            .Where(destination => destination.Location == target)
            .ToList();
        if (targetDestinations.Count > 1)
        {
            _ = destinations.RemoveAll(destination => destination.Location == target);
            destinations.Add(new AccountTransactionDestination(
                null,
                null,
                target,
                targetDestinations.Sum(destination => destination.Amount)));
        }
        UpdateAccountDestinations(destinations);
    }
}
