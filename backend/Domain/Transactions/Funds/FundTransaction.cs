using Domain.Accounts;
using Domain.Funds;
using Domain.Goals;

namespace Domain.Transactions.Funds;

/// <summary>
/// Entity class representing a fund transaction.
/// </summary>
public class FundTransaction : Transaction
{
    private readonly List<FundTransactionDestination> _destinations = [];

    /// <summary>
    /// Source for this Fund Transaction
    /// </summary>
    public FundTransactionSource Source { get; private set; }

    /// <summary>
    /// Destinations for this Fund Transaction
    /// </summary>
    public IReadOnlyCollection<FundTransactionDestination> Destinations => _destinations;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => null;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId != null)
        {
            return [];
        }
        return [Source.Fund.Id, .. Destinations.Select(destination => destination.Fund.Id)];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundTransaction(CreateFundTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Fund)
    {
        Source = request.Source;
        UpdateFundDestinations(request.Destinations);
    }

    /// <summary>
    /// Updates the fund transaction source for this fund transaction.
    /// </summary>
    internal void UpdateFundSource(FundTransactionSource source) => Source = source;

    /// <summary>
    /// Updates the fund transaction destinations for this fund transaction.
    /// </summary>
    internal void UpdateFundDestinations(IReadOnlyCollection<FundTransactionDestination> destinations)
    {
        _destinations.Clear();
        _destinations.AddRange(destinations);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected FundTransaction()
        : base()
    {
        Source = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse) => existingAccountBalance;

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse) => existingAccountBalance;

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        FundBalance newBalance = existingFundBalance;
        if (existingFundBalance.FundId == Source.Fund.Id)
        {
            newBalance = newBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
            newBalance = newBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        FundTransactionDestination? destination = _destinations.FirstOrDefault(d => d.Fund.Id == existingFundBalance.FundId);
        if (destination != null)
        {
            newBalance = newBalance.AddNewPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
            newBalance = newBalance.PostPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
        }
        return newBalance;
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override GoalBalance AddToGoalBalance(GoalBalance existingGoalBalance, bool reverse)
    {
        GoalBalance newBalance = existingGoalBalance;
        if (existingGoalBalance.FundId == Source.Fund.Id)
        {
            decimal amount = reverse ? Amount : -Amount;
            newBalance = newBalance.AddNewPendingAmountAssigned(amount);
            newBalance = newBalance.PostPendingAmountAssigned(amount);
        }
        FundTransactionDestination? destination = _destinations.FirstOrDefault(destination => destination.Fund.Id == existingGoalBalance.FundId);
        if (destination != null)
        {
            decimal amount = reverse ? -destination.Amount : destination.Amount;
            newBalance = newBalance.AddNewPendingAmountAssigned(amount);
            newBalance = newBalance.PostPendingAmountAssigned(amount);
        }
        return newBalance;
    }

    /// <inheritdoc/>
    protected override GoalBalance PostToGoalBalance(GoalBalance existingGoalBalance, AccountId accountId, bool reverse) => existingGoalBalance;
}