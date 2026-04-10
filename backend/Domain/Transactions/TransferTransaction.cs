using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a transfer transaction.
/// </summary>
/// <remarks>
/// A transfer transaction is limited to only moving money between tracked accounts, and it will not affect funds.
/// </remarks>
public class TransferTransaction : Transaction
{
    /// <summary>
    /// Debit Account ID for this Transfer Transaction
    /// </summary>
    public AccountId DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Credit Account ID for this Transfer Transaction
    /// </summary>
    public AccountId CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <inheritdoc/>
    internal override IEnumerable<AccountId> GetAllAffectedAccountIds() => [DebitAccountId, CreditAccountId];

    /// <inheritdoc/>
    internal override DateOnly? GetPostedDateForAccount(AccountId accountId)
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
    internal override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        AccountBalance newAccountBalance = existingAccountBalance;
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            newAccountBalance = newAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        else if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            newAccountBalance = newAccountBalance.AddNewPendingCreditAmount(reverse ? -Amount : Amount);
        }
        return newAccountBalance;
    }

    /// <inheritdoc/>
    internal override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        AccountBalance newAccountBalance = existingAccountBalance;
        if (existingAccountBalance.Account.Id == DebitAccountId)
        {
            newAccountBalance = newAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        else if (existingAccountBalance.Account.Id == CreditAccountId)
        {
            newAccountBalance = newAccountBalance.PostPendingCreditAmount(reverse ? -Amount : Amount);
        }
        return newAccountBalance;
    }

    /// <inheritdoc/>
    internal override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => [];

    /// <inheritdoc/>
    internal override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    internal override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal TransferTransaction(CreateTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Transfer)
    {
        DebitAccountId = request.DebitAccount.Id;
        DebitPostedDate = request.DebitPostedDate;
        CreditAccountId = request.CreditAccount.Id;
        CreditPostedDate = request.CreditPostedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected TransferTransaction()
        : base()
    {
        DebitAccountId = null!;
        CreditAccountId = null!;
    }
}