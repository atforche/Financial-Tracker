using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Entity class representing an income transaction.
/// </summary>
public class IncomeTransaction : Transaction
{
    private readonly List<IncomeTransactionDestination> _destinations = [];

    /// <summary>
    /// Source for this Income Transaction
    /// </summary>
    public IncomeTransactionSource Source { get; private set; }

    /// <summary>
    /// Total tracked amount for this Income Transaction
    /// </summary>
    public decimal TrackedAmount { get; private set; }

    /// <summary>
    /// Destinations for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<IncomeTransactionDestination> Destinations => _destinations;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        if (Source.Account?.Id != null)
        {
            yield return Source.Account.Id;
        }
        foreach (AccountId accountId in Destinations.Select(d => d.Account.Id))
        {
            yield return accountId;
        }
    }

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == Source.Account?.Id)
        {
            return Source.PostedDate;
        }
        if (Destinations.Any(d => d.Account.Id == accountId))
        {
            return Destinations.First(d => d.Account.Id == accountId).PostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null)
        {
            return Destinations.SelectMany(d => d.FundAssignments).Select(f => f.FundId).Distinct();
        }
        if (Destinations.Any(d => d.Account.Id == accountId))
        {
            return Destinations.First(d => d.Account.Id == accountId).FundAssignments.Select(f => f.FundId).Distinct();
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransaction(CreateIncomeTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Income) { }

    /// <summary>
    /// Updates the income source for this income transaction.
    /// </summary>
    internal void UpdateIncomeSource(IncomeTransactionSource incomeSource) => Source = incomeSource;

    /// <summary>
    /// Updates the income destinations for this income transaction.
    /// </summary>
    internal void UpdateIncomeDestinations(IReadOnlyCollection<IncomeTransactionDestination> incomeDestinations)
    {
        _destinations.Clear();
        _destinations.AddRange(incomeDestinations);
        TrackedAmount = Destinations.Where(d => d.Account.Type.IsTracked()).Sum(d => d.Amount);
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
        IncomeTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account.Id == accountId);
        _ = (destination?.PostedDate = postedDate);
    }

    /// <summary>
    /// Clears all posted dates for this transaction.
    /// </summary>
    internal void ClearPostedDates()
    {
        Source.PostedDate = null;
        foreach (IncomeTransactionDestination destination in _destinations)
        {
            destination.PostedDate = null;
        }
    }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected IncomeTransaction(CreateIncomeTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        Source = request.Source;
        UpdateIncomeDestinations(request.Destinations);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
        : base()
    {
        Source = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        IncomeTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Account.Id == existingAccountBalance.Account.Id);
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
        var fundAmounts = _destinations.Where(d => d.Account.Id == accountId)
            .SelectMany(d => d.FundAssignments)
            .Where(fundAmount => fundAmount.FundId == existingFundBalance.Fund.Id)
            .ToList();
        if (fundAmounts.Count == 0)
        {
            return existingFundBalance;
        }
        decimal amount = fundAmounts.Sum(f => f.Amount);
        return existingFundBalance.Credit(reverse ? -amount : amount);
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
        decimal amount = _destinations.Where(destination => destination.Account.Id == accountId)
            .SelectMany(destination => destination.FundAssignments)
            .Where(assignment => assignment.FundId == existingTotals.FundId).Sum(assignment => assignment.Amount);
        return amount == 0 ? existingTotals : existingTotals.Assign(reverse ? -amount : amount);
    }
}