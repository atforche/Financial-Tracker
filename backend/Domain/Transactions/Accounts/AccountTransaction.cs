using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Entity class representing an account transaction.
/// </summary>
/// <remarks>
/// An account transaction represents one of the following scenarios:
///     1. A transaction that only debits an untracked account
///     2. A transaction that only credits an untracked account
///     3. A transaction that moves money between two untracked accounts
///     4. A transaction that moves money between two tracked accounts
/// </remarks>
public class AccountTransaction : Transaction
{
    /// <summary>
    /// Debit Account ID for this Account Transaction
    /// </summary>
    public AccountId? DebitAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Account Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Credit Account ID for this Account Transaction
    /// </summary>
    public AccountId? CreditAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Account Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

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
    internal AccountTransaction(CreateAccountTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Account)
    {
        DebitAccountId = request.DebitAccount?.Id;
        CreditAccountId = request.CreditAccount?.Id;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected AccountTransaction()
        : base()
    {
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