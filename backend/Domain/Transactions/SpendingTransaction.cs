using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a spending transaction.
/// </summary>
public class SpendingTransaction : Transaction
{
    private readonly List<FundAmount> _fundAmounts = [];

    /// <summary>
    /// Account ID for this Spending Transaction
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Posted Date for this Spending Transaction
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Fund Amounts for this Spending Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [AccountId];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => accountId == AccountId ? PostedDate : null;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null || accountId == AccountId)
        {
            return _fundAmounts.Select(f => f.FundId);
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingTransaction(CreateSpendingTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Spending) { }

    /// <summary>
    /// Updates the fund amounts for this Spending Transaction
    /// </summary>
    internal void UpdateFundAmounts(IReadOnlyCollection<FundAmount> fundAmounts)
    {
        _fundAmounts.Clear();
        _fundAmounts.AddRange(fundAmounts);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected SpendingTransaction()
        : base()
    {
        AccountId = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        return existingAccountBalance.PostPendingCreditAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        FundAmount? fundAmount = _fundAmounts.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null)
        {
            return existingFundBalance;
        }
        return existingFundBalance.AddNewPendingAmountSpent(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        FundAmount? fundAmount = _fundAmounts.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null || accountId != AccountId)
        {
            return existingFundBalance;
        }
        return existingFundBalance.PostPendingAmountSpent(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected SpendingTransaction(CreateSpendingTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        AccountId = request.Account.Id;
        PostedDate = request.PostedDate;
        _fundAmounts.AddRange(request.FundAssignments);
    }
}