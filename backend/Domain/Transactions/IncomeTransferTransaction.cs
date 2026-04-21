using Domain.Accounts;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an income transfer transaction.
/// </summary>
/// <remarks>
/// An income transfer transaction represents money coming into a tracked account from an untracked account.
/// The credit to the tracked account can affect funds, but the debit from the untracked account can not.
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

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => base.GetAllAffectedAccountIds().Append(DebitAccountId);

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == DebitAccountId)
        {
            return DebitPostedDate;
        }
        return base.GetPostedDateForAccount(accountId);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransferTransaction(CreateIncomeTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.IncomeTransfer)
    {
        DebitAccountId = request.DebitAccount.Id;
        DebitPostedDate = request.DebitPostedDate;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return base.AddToAccountBalance(existingAccountBalance, reverse);
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return base.PostToAccountBalance(existingAccountBalance, reverse);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeTransferTransaction()
        : base()
    {
        DebitAccountId = null!;
    }
}