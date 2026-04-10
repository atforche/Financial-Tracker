using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an income transaction.
/// </summary>
public class IncomeTransaction : Transaction
{
    /// <summary>
    /// Account ID for this Income Transaction
    /// </summary>
    public AccountId AccountId { get; private set; }

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
    internal override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => [];

    /// <inheritdoc/>
    internal override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    internal override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
        : base()
    {
        AccountId = null!;
    }
}