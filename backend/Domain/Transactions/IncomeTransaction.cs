using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an income transaction.
/// </summary>
public class IncomeTransaction : Transaction
{
    private readonly List<FundAmount> _fundAmounts = [];

    /// <summary>
    /// Account ID for this Income Transaction
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Fund Amounts for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <summary>
    /// Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransaction(CreateIncomeTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Income) { }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    internal IncomeTransaction(CreateIncomeTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        AccountId = request.Account.Id;
        PostedDate = request.PostedDate;
        GeneratedByAccountId = request.IsInitialTransactionForAccount ? AccountId : null;
        _fundAmounts.AddRange(request.FundAssignments);
    }

    /// <inheritdoc/>
    internal override IEnumerable<AccountId> GetAllAffectedAccountIds() => [AccountId];

    /// <inheritdoc/>
    internal override DateOnly? GetPostedDateForAccount(AccountId accountId) =>
        accountId == AccountId ? PostedDate : null;

    /// <inheritdoc/>
    internal override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    internal override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    internal override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null || accountId == AccountId)
        {
            return _fundAmounts.Select(f => f.FundId);
        }
        return [];
    }

    /// <inheritdoc/>
    internal override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        FundAmount? fundAmount = _fundAmounts.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null)
        {
            return existingFundBalance;
        }
        return existingFundBalance.AddNewPendingCreditAmount(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <inheritdoc/>
    internal override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        FundAmount? fundAmount = _fundAmounts.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null || accountId != AccountId)
        {
            return existingFundBalance;
        }
        return existingFundBalance.PostPendingCredit(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <summary>
    /// Updates the fund amounts for this Income Transaction
    /// </summary>
    internal void UpdateFundAmounts(IReadOnlyCollection<FundAmount> fundAmounts)
    {
        _fundAmounts.Clear();
        _fundAmounts.AddRange(fundAmounts);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
        : base()
    {
        AccountId = null!;
    }
}