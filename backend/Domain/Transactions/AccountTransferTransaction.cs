using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing an account transfer transaction.
/// </summary>
/// <remarks>
/// An account transfer transaction represents money moving between two tracked accounts. It can not affect funds.
/// </remarks>
public class AccountTransferTransaction : Transaction
{
    /// <summary>
    /// Debit Account ID for this Account Transfer Transaction
    /// </summary>
    public AccountId DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Credit Account ID for this Account Transfer Transaction
    /// </summary>
    public AccountId CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [DebitAccountId, CreditAccountId];

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
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) => [];

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountTransferTransaction(CreateAccountTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.AccountTransfer)
    {
        DebitAccountId = request.DebitAccount.Id;
        DebitPostedDate = request.DebitPostedDate;
        CreditAccountId = request.CreditAccount.Id;
        CreditPostedDate = request.CreditPostedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected AccountTransferTransaction()
        : base()
    {
        DebitAccountId = null!;
        CreditAccountId = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
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
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
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
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) => existingFundBalance;

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;
}