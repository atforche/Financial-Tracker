using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Spending;

/// <summary>
/// Entity class representing a spending transaction.
/// </summary>
/// <remarks>
/// A spending transaction represents money going out of a tracked account to some external destination. 
/// The debit from the tracked account must be assigned to some combination of funds.
/// </remarks>
public class SpendingTransaction : Transaction
{
    private readonly List<FundAmount> _fundAssignments = [];

    /// <summary>
    /// Debit Account ID for this Spending Transaction
    /// </summary>
    public AccountId DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Spending Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Credit Account ID for this Spending Transaction
    /// </summary>
    public AccountId? CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <summary>
    /// Fund assignments for this Spending Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        yield return DebitAccountId;
        if (CreditAccountId != null)
        {
            yield return CreditAccountId;
        }
    }

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == DebitAccountId)
        {
            return DebitPostedDate;
        }
        if (accountId == CreditAccountId)
        {
            return CreditPostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null || accountId == DebitAccountId)
        {
            return _fundAssignments.Select(f => f.FundId);
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingTransaction(CreateSpendingTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Spending) { }

    /// <summary>
    /// Updates the fund assignments for this Spending Transaction
    /// </summary>
    internal void UpdateFundAssignments(IReadOnlyCollection<FundAmount> fundAssignments)
    {
        _fundAssignments.Clear();
        _fundAssignments.AddRange(fundAssignments);
    }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected SpendingTransaction(CreateSpendingTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        DebitAccountId = request.DebitAccount.Id;
        CreditAccountId = request.CreditAccount?.Id;
        GeneratedByAccountId = request.IsInitialTransactionForAccount ? request.DebitAccount.Id : null;
        _fundAssignments.AddRange(request.FundAssignments);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected SpendingTransaction()
        : base()
    {
        DebitAccountId = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            return existingAccountBalance.PostPendingCreditAmount(reverse ? -Amount : Amount);
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
        return existingFundBalance.AddNewPendingAmountSpent(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        FundAmount? fundAmount = _fundAssignments.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null || accountId != DebitAccountId)
        {
            return existingFundBalance;
        }
        return existingFundBalance.PostPendingAmountSpent(reverse ? -fundAmount.Amount : fundAmount.Amount);
    }
}