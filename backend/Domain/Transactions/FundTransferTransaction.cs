using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a fund transfer transaction.
/// </summary>
/// <remarks>
/// A fund transfer transaction represents money moving between two funds. It can not affect accounts.
/// </remarks>
public class FundTransferTransaction : Transaction
{
    /// <summary>
    /// Debit Fund ID for this Fund Transfer Transaction
    /// </summary>
    public FundId DebitFundId { get; private set; }

    /// <summary>
    /// Credit Fund ID for this Fund Transfer Transaction
    /// </summary>
    public FundId CreditFundId { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds() => [];

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId) => null;

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId != null)
        {
            return [];
        }
        return [DebitFundId, CreditFundId];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundTransferTransaction(CreateTransferTransactionRequest request, int sequence)
        : base(request, sequence, TransactionType.Transfer)
    {
        DebitFundId = request.DebitFund.Id;
        CreditFundId = request.CreditFund.Id;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected FundTransferTransaction()
        : base()
    {
        DebitFundId = null!;
        CreditFundId = null!;
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse) => existingAccountBalance;

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse) => existingAccountBalance;

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        FundBalance newBalance = existingFundBalance;
        if (existingFundBalance.FundId == DebitFundId)
        {
            newBalance = newBalance.AddNewPendingAmountAssigned(reverse ? Amount : -Amount);
            newBalance = newBalance.PostPendingAmountAssigned(reverse ? Amount : -Amount);
        }
        else if (existingFundBalance.FundId == CreditFundId)
        {
            newBalance = newBalance.AddNewPendingAmountAssigned(reverse ? -Amount : Amount);
            newBalance = newBalance.PostPendingAmountAssigned(reverse ? -Amount : Amount);
        }
        return newBalance;
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse) => existingFundBalance;
}