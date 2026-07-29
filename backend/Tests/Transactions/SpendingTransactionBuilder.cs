using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Builds a spending transaction with one location destination and fund assignment.
/// </summary>
internal sealed class SpendingTransactionBuilder(TestApiClient apiClient)
{
    private AccountingPeriodHandle? _period;
    private DateOnly _date;
    private decimal _amount;
    private AccountHandle? _source;
    private string? _location;
    private FundHandle? _fund;

    /// <summary>
    /// Sets the accounting period for the transaction.
    /// </summary>
    public SpendingTransactionBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Sets the transaction date.
    /// </summary>
    public SpendingTransactionBuilder On(DateOnly date)
    {
        _date = date;
        return this;
    }

    /// <summary>
    /// Sets the transaction amount.
    /// </summary>
    public SpendingTransactionBuilder For(decimal amount)
    {
        _amount = amount;
        return this;
    }

    /// <summary>
    /// Sets the account from which the spending originates.
    /// </summary>
    public SpendingTransactionBuilder From(AccountHandle account)
    {
        _source = account;
        return this;
    }

    /// <summary>
    /// Sets the location destination and its fund assignment.
    /// </summary>
    public SpendingTransactionBuilder To(string location, FundHandle fund)
    {
        _location = location;
        _fund = fund;
        return this;
    }

    /// <summary>
    /// Creates the transaction.
    /// </summary>
    public async Task<TransactionHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.");
        AccountHandle source = _source ?? throw new InvalidOperationException("A spending transaction must have a source account.");
        string location = _location ?? throw new InvalidOperationException("A spending transaction must have a destination location.");
        FundHandle fund = _fund ?? throw new InvalidOperationException("A spending transaction must have a fund assignment.");
        CreateTransactionModel request = new CreateSpendingTransactionModel
        {
            AccountingPeriodId = period.Id,
            Date = _date,
            Description = location,
            Amount = _amount,
            Source = new CreateSpendingTransactionSourceModel
            {
                AccountId = source.Id
            },
            Destinations = [new CreateSpendingTransactionDestinationModel
            {
                Location = location,
                Amount = _amount,
                FundAssignments = [new CreateFundAmountModel
                {
                    FundId = fund.Id,
                    Amount = _amount
                }]
            }]
        };
        CreateTransactionResultModel model = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        return new TransactionHandle(model.Id);
    }

    /// <summary>
    /// Updates an unposted spending transaction with this builder's values.
    /// </summary>
    public async Task UpdateAsync(TransactionHandle transaction)
    {
        AccountHandle source = _source ?? throw new InvalidOperationException("A spending transaction must have a source account.");
        string location = _location ?? throw new InvalidOperationException("A spending transaction must have a destination location.");
        FundHandle fund = _fund ?? throw new InvalidOperationException("A spending transaction must have a fund assignment.");
        UpdateTransactionModel request = new UpdateSpendingTransactionModel
        {
            Date = _date,
            Description = location,
            Amount = _amount,
            Source = new UpdateSpendingTransactionSourceModel
            {
                AccountId = source.Id
            },
            Destinations = [new UpdateSpendingTransactionDestinationModel
            {
                Location = location,
                Amount = _amount,
                FundAssignments = [new CreateFundAmountModel
                {
                    FundId = fund.Id,
                    Amount = _amount
                }]
            }]
        };
        await apiClient.PostAsync($"/transactions/{transaction.Id}", request);
    }
}