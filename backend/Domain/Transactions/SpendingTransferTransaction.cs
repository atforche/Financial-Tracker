using Domain.Accounts;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a spending transfer transaction.
/// </summary>
/// <remarks>
/// A spending transfer transaction represents money going out of a tracked account to an untracked account.
/// The debit from the tracked account must be assigned to some combination of funds, but the credit to the untracked account can not affect funds.
/// </remarks>
public class SpendingTransferTransaction : SpendingTransaction
{
    /// <summary>
    /// Credit Account ID for this Spending Transfer Transaction
    /// </summary>
    public AccountId CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [AccountId, CreditAccountId];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == CreditAccountId)
        {
            return CreditPostedDate;
        }
        return base.GetPostedDateForAccount(accountId);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingTransferTransaction(CreateSpendingTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.SpendingTransfer)
    {
        CreditAccountId = request.CreditAccount.Id;
        CreditPostedDate = request.CreditPostedDate;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return base.AddToAccountBalance(existingAccountBalance, reverse);
        }
        return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != CreditAccountId)
        {
            return base.PostToAccountBalance(existingAccountBalance, reverse);
        }
        return existingAccountBalance.PostPendingCreditAmount(reverse ? -Amount : Amount);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private SpendingTransferTransaction()
        : base()
    {
        CreditAccountId = null!;
    }
}