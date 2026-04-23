using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Income;

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
    /// Credit Account ID for this Income Transaction
    /// </summary>
    public AccountId CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Income Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <summary>
    /// Debit Account ID for this Income Transaction
    /// </summary>
    public AccountId? DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Income Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Fund assignments for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        if (DebitAccountId != null)
        {
            yield return DebitAccountId;
        }
        yield return CreditAccountId;
    }

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == DebitAccountId)
        {
            return DebitPostedDate;
        }
        else if (accountId == CreditAccountId)
        {
            return CreditPostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null || accountId == CreditAccountId)
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
        CreditAccountId = request.CreditAccount.Id;
        CreditPostedDate = request.CreditPostedDate;
        DebitAccountId = request.DebitAccount?.Id;
        DebitPostedDate = request.DebitPostedDate;
        GeneratedByAccountId = request.IsInitialTransactionForAccount ? CreditAccountId : null;
        _fundAssignments.AddRange(request.FundAssignments);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
        : base()
    {
        CreditAccountId = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -Amount : Amount);
        }
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            return existingAccountBalance.PostPendingCreditAmount(reverse ? -Amount : Amount);
        }
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
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
        if (fundAmount == null || accountId != CreditAccountId)
        {
            return existingFundBalance;
        }
        return existingFundBalance.PostPendingAmountAssigned(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }
}