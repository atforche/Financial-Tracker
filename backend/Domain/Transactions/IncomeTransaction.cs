using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an income transaction.
/// </summary>
/// <remarks>
/// An income transaction represents money coming into a tracked account from some external source. 
/// The credit to the tracked account can optionally be directly assigned to funds.
/// </remarks>
public class IncomeTransaction : Transaction
{
    private readonly List<FundAmount> _fundAssignments = [];

    /// <summary>
    /// Account ID for this Income Transaction
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Fund assignments for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [AccountId];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => accountId == AccountId ? PostedDate : null;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null || accountId == AccountId)
        {
            return _fundAssignments.Select(f => f.FundId);
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransaction(CreateIncomeTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Income) { }

    /// <summary>
    /// Updates the fund assignments for this Income Transaction
    /// </summary>
    internal void UpdateFundAssignments(IReadOnlyCollection<FundAmount> fundAssignments)
    {
        _fundAssignments.Clear();
        _fundAssignments.AddRange(fundAssignments);
    }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected IncomeTransaction(CreateIncomeTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        AccountId = request.Account.Id;
        PostedDate = request.PostedDate;
        GeneratedByAccountId = request.IsInitialTransactionForAccount ? AccountId : null;
        _fundAssignments.AddRange(request.FundAssignments);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
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
        return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        FundAmount? fundAmount = _fundAssignments.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null)
        {
            return existingFundBalance;
        }
        return existingFundBalance.AddNewPendingAmountAssigned(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        FundAmount? fundAmount = _fundAssignments.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null || accountId != AccountId)
        {
            return existingFundBalance;
        }
        return existingFundBalance.PostPendingAmountAssigned(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }
}