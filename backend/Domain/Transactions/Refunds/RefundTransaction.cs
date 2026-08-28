using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Locations;

namespace Domain.Transactions.Refunds;

/// <summary>
/// A reversal of spending from untracked sources into one tracked account.
/// </summary>
public class RefundTransaction : Transaction
{
    private readonly List<RefundTransactionSource> _sources = [];

    /// <summary>
    /// Sources for this refund transaction.
    /// </summary>
    public IReadOnlyCollection<RefundTransactionSource> Sources => _sources;

    /// <summary>
    /// Destination for this refund transaction.
    /// </summary>
    public RefundTransactionDestination Destination { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        foreach (AccountId id in Sources.Where(source => source.Account != null).Select(source => source.Account!.Id))
        {
            yield return id;
        }
        yield return Destination.Account.Id;
    }

    /// <inheritdoc/>
    public override IEnumerable<LocationId> GetAllAffectedLocationIds() => Sources
        .Where(source => source.Location != null)
        .Select(source => source.Location!.Id)
        .Distinct();

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => Destination.Account.Id == accountId
        ? Destination.PostedDate
        : Sources.FirstOrDefault(source => source.Account?.Id == accountId)?.PostedDate;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => GetSourcesForAccount(accountId)
        .SelectMany(source => source.FundAssignments)
        .Select(amount => amount.FundId)
        .Distinct();

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal RefundTransaction(CreateRefundTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Refund)
    {
        UpdateRefundSources(request.Sources);
        Destination = request.Destination;
    }

    /// <summary>
    /// Updates the refund sources for this refund transaction.
    /// </summary>
    internal void UpdateRefundSources(IReadOnlyCollection<RefundTransactionSource> sources)
    {
        _sources.Clear();
        _sources.AddRange(sources);
    }

    /// <summary>
    /// Updates the refund destination for this refund transaction.
    /// </summary>
    internal void UpdateRefundDestination(RefundTransactionDestination destination) => Destination = destination;

    /// <summary>
    /// Sets the posted date for a specific account affected by this transaction.
    /// </summary>
    internal void SetPostedDate(AccountId accountId, DateOnly? postedDate)
    {
        if (Destination.Account.Id == accountId)
        {
            Destination.PostedDate = postedDate;
            return;
        }
        RefundTransactionSource? source = _sources.FirstOrDefault(item => item.Account?.Id == accountId);
        _ = source?.PostedDate = postedDate;
    }

    /// <summary>
    /// Clears all posted dates for this transaction.
    /// </summary>
    internal void ClearPostedDates()
    {
        Destination.PostedDate = null;
        foreach (RefundTransactionSource source in _sources)
        {
            source.PostedDate = null;
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    protected RefundTransaction()
        : base()
    {
        Destination = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (Destination.Account.Id == existingAccountBalance.Account.Id)
        {
            return existingAccountBalance.Credit(reverse ? -Amount : Amount);
        }
        RefundTransactionSource? source = Sources.FirstOrDefault(item => item.Account?.Id == existingAccountBalance.Account.Id);
        return source == null ? existingAccountBalance : existingAccountBalance.Debit(reverse ? -source.Amount : source.Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        decimal amount = GetSourcesForAccount(accountId)
            .SelectMany(source => source.FundAssignments)
            .Where(assignment => assignment.FundId == existingFundBalance.Fund.Id)
            .Sum(assignment => assignment.Amount);
        return amount == 0 ? existingFundBalance : existingFundBalance.Credit(reverse ? -amount : amount);
    }

    /// <inheritdoc/>
    protected override FundGoalTotals AddToFundGoalTotals(FundGoalTotals existingTotals, bool reverse) => existingTotals;

    /// <inheritdoc/>
    protected override FundGoalTotals PostToFundGoalTotals(FundGoalTotals existingTotals, AccountId accountId, bool reverse)
    {
        decimal amount = GetSourcesForAccount(accountId)
            .SelectMany(source => source.FundAssignments)
            .Where(assignment => assignment.FundId == existingTotals.FundId)
            .Sum(assignment => assignment.Amount);
        return amount == 0 ? existingTotals : existingTotals.Spend(reverse ? amount : -amount);
    }

    private IEnumerable<RefundTransactionSource> GetSourcesForAccount(AccountId? accountId) => accountId == null
        ? Sources
        : accountId == Destination.Account.Id
            ? Sources.Where(source => source.Account == null)
            : Sources.Where(source => source.Account?.Id == accountId);

    /// <inheritdoc/>
    internal override void ReplaceLocation(Location source, Location target)
    {
        var sources = Sources
            .Select(item => item.Location == source
                ? new RefundTransactionSource(item.Account, item.PostedDate, target, item.Amount, item.FundAssignments)
                : item)
            .ToList();
        var matches = sources
            .Where(item => item.Location == target)
            .ToList();
        if (matches.Count > 1)
        {
            _ = sources.RemoveAll(item => item.Location == target);
            sources.Add(new RefundTransactionSource(
                null,
                null,
                target,
                matches.Sum(item => item.Amount),
                matches.SelectMany(item => item.FundAssignments)
                    .GroupBy(item => item.FundId)
                    .Select(group => new FundAmount { FundId = group.Key, Amount = group.Sum(item => item.Amount) })));
        }
        UpdateRefundSources(sources);
    }
}
