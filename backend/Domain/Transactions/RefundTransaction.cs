using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a refund transaction.
/// </summary>
public class RefundTransaction : Transaction
{
    /// <summary>
    /// Transaction for this Refund Transaction
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Posted Date for the Debit Account of this Refund Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; internal set; }

    /// <summary>
    /// Posted Date for the Credit Account of this Refund Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public RefundTransaction(CreateRefundTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Refund)
    {
        Transaction = request.Transaction;
        DebitPostedDate = request.DebitPostedDate;
        CreditPostedDate = request.CreditPostedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    public RefundTransaction()
        : base()
    {
        Transaction = null!;
    }

    /// <inheritdoc/>
    internal override IEnumerable<AccountId> GetAllAffectedAccountIds() => Transaction.GetAllAffectedAccountIds();

    /// <inheritdoc/>
    internal override DateOnly? GetPostedDateForAccount(AccountId accountId) => null;

    /// <inheritdoc/>
    internal override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse) =>
        Transaction.AddToAccountBalance(existingAccountBalance, !reverse);

    /// <inheritdoc/>
    internal override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse) =>
        Transaction.PostToAccountBalance(existingAccountBalance, !reverse);

    /// <inheritdoc/>
    internal override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId) =>
        Transaction.GetAllAffectedFundIds(accountId);

    /// <inheritdoc/>
    internal override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse) =>
        Transaction.AddToFundBalance(existingFundBalance, !reverse);

    /// <inheritdoc/>
    internal override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) =>
         Transaction.PostToFundBalance(existingFundBalance, accountId, !reverse);
}