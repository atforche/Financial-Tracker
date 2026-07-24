using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;

namespace Domain.Transactions.Spending;

/// <summary>
/// Entity class representing a spending transaction.
/// </summary>
public class SpendingTransaction : Transaction
{
    private readonly List<SpendingTransactionDestination> _destinations = [];

    /// <summary>
    /// Source for this Spending Transaction
    /// </summary>
    public SpendingTransactionSource Source { get; private set; }

    /// <summary>
    /// Destinations for this Spending Transaction
    /// </summary>
    public IReadOnlyCollection<SpendingTransactionDestination> Destinations => _destinations;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        yield return Source.Account.Id;
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
        if (accountId == Source.Account.Id)
        {
            return Source.PostedDate;
        }
        if (Destinations.Any(d => d.Account?.Id == accountId))
        {
            return Destinations.First(d => d.Account?.Id == accountId).PostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null)
        {
            return Destinations
                .SelectMany(d => d.FundAssignments)
                .Select(f => f.FundId)
                .Distinct();
        }
        if (accountId == Source.Account.Id)
        {
            return Destinations
                .Where(d => d.Account == null)
                .SelectMany(d => d.FundAssignments)
                .Select(f => f.FundId)
                .Distinct();
        }
        if (Destinations.Any(d => d.Account?.Id == accountId))
        {
            return Destinations.First(d => d.Account?.Id == accountId).FundAssignments.Select(f => f.FundId).Distinct();
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingTransaction(CreateSpendingTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Spending) { }

    /// <summary>
    /// Updates the spending source for this Spending Transaction
    /// </summary>
    internal void UpdateSpendingSource(SpendingTransactionSource source) => Source = source;

    /// <summary>
    /// Updates the spending destinations for this Spending Transaction
    /// </summary>
    internal void UpdateSpendingDestinations(IReadOnlyCollection<SpendingTransactionDestination> destinations)
    {
        _destinations.Clear();
        _destinations.AddRange(destinations);
    }

    /// <summary>
    /// Sets the posted date for a specific account affected by this transaction.
    /// </summary>
    internal void SetPostedDate(AccountId accountId, DateOnly? postedDate)
    {
        if (accountId == Source.Account.Id)
        {
            Source.PostedDate = postedDate;
            return;
        }
        SpendingTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == accountId);
        _ = (destination?.PostedDate = postedDate);
    }

    /// <summary>
    /// Clears all posted dates for this transaction.
    /// </summary>
    internal void ClearPostedDates()
    {
        Source.PostedDate = null;
        foreach (SpendingTransactionDestination destination in _destinations)
        {
            destination.PostedDate = null;
        }
    }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected SpendingTransaction(CreateSpendingTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        Source = request.Source;
        UpdateSpendingDestinations(request.Destinations);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected SpendingTransaction()
        : base()
    {
        Source = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        SpendingTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account?.Id == existingAccountBalance.Account.Id);
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
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        var fundAmounts = _destinations.SelectMany(d => d.FundAssignments)
            .Where(fundAmount => fundAmount.FundId == existingFundBalance.Fund.Id)
            .ToList();
        if (fundAmounts.Count == 0)
        {
            return existingFundBalance;
        }
        decimal amount = fundAmounts.Sum(f => f.Amount);
        return existingFundBalance;
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        IEnumerable<FundAmount> fundAmounts = accountId == Source.Account.Id
            ? _destinations
                .Where(d => d.Account == null)
                .SelectMany(d => d.FundAssignments)
            : _destinations
                .Where(d => d.Account?.Id == accountId)
                .SelectMany(d => d.FundAssignments);

        fundAmounts = fundAmounts.Where(fundAmount => fundAmount.FundId == existingFundBalance.Fund.Id);
        decimal amount = fundAmounts.Sum(f => f.Amount);
        if (amount == 0)
        {
            return existingFundBalance;
        }
        return existingFundBalance.Debit(reverse ? -amount : amount);
    }

    /// <inheritdoc/>
    protected override FundPlanTotals AddToFundPlanTotals(FundPlanTotals existingTotals, bool reverse)
    {
        decimal amount = _destinations.SelectMany(destination => destination.FundAssignments)
            .Where(assignment => assignment.FundId == existingTotals.FundId).Sum(assignment => assignment.Amount);
        return existingTotals;
    }

    /// <inheritdoc/>
    protected override FundPlanTotals PostToFundPlanTotals(FundPlanTotals existingTotals, AccountId accountId, bool reverse)
    {
        IEnumerable<FundAmount> assignments = accountId == Source.Account.Id
            ? _destinations.Where(destination => destination.Account == null).SelectMany(destination => destination.FundAssignments)
            : _destinations.Where(destination => destination.Account?.Id == accountId).SelectMany(destination => destination.FundAssignments);
        decimal amount = assignments.Where(assignment => assignment.FundId == existingTotals.FundId).Sum(assignment => assignment.Amount);
        return amount == 0 ? existingTotals : existingTotals.Spend(reverse ? -amount : amount);
    }
}