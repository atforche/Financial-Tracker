using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builder for Balance Adjustments. The amount is a signed change to the account's balance as it is displayed.
/// </summary>
internal sealed class BalanceAdjustmentBuilder(TestApiClient apiClient) : TransactionBuilder<BalanceAdjustmentBuilder>(apiClient)
{
    private AccountHandle? _account;

    /// <summary>
    /// Sets the account whose balance is adjusted.
    /// </summary>
    public BalanceAdjustmentBuilder Of(AccountHandle account)
    {
        _account = account;
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateBalanceAdjustmentModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = "Change in value",
        Amount = Amount,
        AccountId = AccountId,
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateBalanceAdjustmentModel
    {
        Date = Date,
        Description = "Change in value",
        Amount = Amount,
        AccountId = AccountId,
    };

    private Guid AccountId => (_account ?? throw new InvalidOperationException("A balance adjustment must have an account.")).Id;
}
