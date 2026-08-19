using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Builds a fund-to-fund transfer transaction.
/// </summary>
internal sealed class FundTransactionBuilder(TestApiClient apiClient)
{
    private AccountingPeriodHandle? _period;
    private DateOnly _date;
    private decimal _amount;
    private FundHandle? _source;
    private FundHandle? _destination;

    /// <summary>
    /// Sets the accounting period for the transaction.
    /// </summary>
    public FundTransactionBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Sets the transaction date.
    /// </summary>
    public FundTransactionBuilder On(DateOnly date)
    {
        _date = date;
        return this;
    }

    /// <summary>
    /// Sets the transfer amount.
    /// </summary>
    public FundTransactionBuilder For(decimal amount)
    {
        _amount = amount;
        return this;
    }

    /// <summary>
    /// Sets the source fund.
    /// </summary>
    public FundTransactionBuilder From(FundHandle fund)
    {
        _source = fund;
        return this;
    }

    /// <summary>
    /// Sets the destination fund.
    /// </summary>
    public FundTransactionBuilder To(FundHandle fund)
    {
        _destination = fund;
        return this;
    }

    /// <summary>
    /// Creates the transaction.
    /// </summary>
    public async Task<TransactionHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.");
        CreateTransactionModel request = new CreateFundTransactionModel
        {
            AccountingPeriodId = period.Id,
            Date = _date,
            Description = "Fund transfer",
            Amount = _amount,
            Source = new CreateFundTransactionSourceModel
            {
                FundId = GetSource().Id
            },
            Destinations = [new CreateFundTransactionDestinationModel
            {
                FundId = GetDestination().Id,
                Amount = _amount
            }]
        };
        CreateTransactionResultModel model = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        return new TransactionHandle(model.Id);
    }

    /// <summary>
    /// Updates a fund transfer transaction with this builder's values.
    /// </summary>
    public Task UpdateAsync(TransactionHandle transaction)
    {
        UpdateTransactionModel request = new UpdateFundTransactionModel
        {
            Date = _date,
            Description = "Fund transfer",
            Amount = _amount,
            Source = new UpdateFundTransactionSourceModel
            {
                FundId = GetSource().Id
            },
            Destinations = [new UpdateFundTransactionDestinationModel
            {
                FundId = GetDestination().Id,
                Amount = _amount
            }]
        };
        return apiClient.PostAsync($"/transactions/{transaction.Id}", request);
    }

    private FundHandle GetSource() => _source ?? throw new InvalidOperationException("A fund transaction must have a source fund.");

    private FundHandle GetDestination() => _destination ?? throw new InvalidOperationException("A fund transaction must have a destination fund.");
}
