using Domain.Accounts;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an income transfer transaction.
/// </summary>
/// <remarks>
/// An income transaction transaction is limited to money money from an untracked account to a tracked account.
/// The credit to the tracked account will affect funds, but the debit from the untracked account will not.
/// </remarks>
public class IncomeTransferTransaction : IncomeTransaction
{
    /// <summary>
    /// Debit Account ID for this Income Transfer Transaction
    /// </summary>
    public AccountId DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Income Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransferTransaction(CreateIncomeTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.IncomeTransfer)
    {
        DebitAccountId = request.DebitAccount.Id;
        DebitPostedDate = request.DebitPostedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeTransferTransaction()
        : base()
    {
        DebitAccountId = null!;
    }

    /// <inheritdoc/>
    internal override IEnumerable<AccountId> GetAllAffectedAccountIds() => [DebitAccountId, AccountId];

    /// <inheritdoc/>
    internal override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == DebitAccountId)
        {
            return DebitPostedDate;
        }
        return base.GetPostedDateForAccount(accountId);
    }

    /// <inheritdoc/>
    internal override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != DebitAccountId)
        {
            return base.AddToAccountBalance(existingAccountBalance, reverse);
        }
        return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
    }

    /// <inheritdoc/>
    internal override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id != DebitAccountId)
        {
            return base.PostToAccountBalance(existingAccountBalance, reverse);
        }
        return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
    }
}