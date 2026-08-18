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
/// Builds an income transaction with one location source, account destination, and fund assignment.
/// </summary>
internal sealed class IncomeTransactionBuilder(TestApiClient apiClient)
{
    private AccountingPeriodHandle? _period;
    private DateOnly _date;
    private decimal _amount;
    private string? _location;
    private AccountHandle? _destination;
    private FundHandle? _fund;

    /// <summary>
    /// Sets the accounting period for the transaction.
    /// </summary>
    public IncomeTransactionBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Sets the transaction date.
    /// </summary>
    public IncomeTransactionBuilder On(DateOnly date)
    {
        _date = date;
        return this;
    }

    /// <summary>
    /// Sets the income amount.
    /// </summary>
    public IncomeTransactionBuilder For(decimal amount)
    {
        _amount = amount;
        return this;
    }

    /// <summary>
    /// Sets the income source location.
    /// </summary>
    public IncomeTransactionBuilder From(string location)
    {
        _location = location;
        return this;
    }

    /// <summary>
    /// Sets the destination account and its fund assignment.
    /// </summary>
    public IncomeTransactionBuilder To(AccountHandle account, FundHandle fund)
    {
        _destination = account;
        _fund = fund;
        return this;
    }

    /// <summary>
    /// Creates the transaction.
    /// </summary>
    public async Task<TransactionHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.");
        CreateTransactionModel request = CreateRequest(period);
        CreateTransactionResultModel model = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        return new TransactionHandle(model.Id);
    }

    /// <summary>
    /// Updates an unposted income transaction with this builder's values.
    /// </summary>
    public Task UpdateAsync(TransactionHandle transaction)
    {
        UpdateTransactionModel request = new UpdateIncomeTransactionModel
        {
            Date = _date,
            Description = GetLocation(),
            Amount = _amount,
            Source = new UpdateIncomeTransactionSourceModel
            {
                Location = GetLocation() is { } location ? new Models.Locations.LocationInputModel { NewLocationName = location } : null,
                IncomeLines = [new UpdateIncomeLineModel
                {
                    Description = GetLocation(),
                    Amount = _amount
                }],
                IncomeDeductions = []
            },
            Destinations = [CreateUpdateDestination()]
        };
        return apiClient.PostAsync($"/transactions/{transaction.Id}", request);
    }

    private CreateIncomeTransactionModel CreateRequest(AccountingPeriodHandle period) => new()
    {
        AccountingPeriodId = period.Id,
        Date = _date,
        Description = GetLocation(),
        Amount = _amount,
        Source = new CreateIncomeTransactionSourceModel
        {
            Location = GetLocation() is { } location ? new Models.Locations.LocationInputModel { NewLocationName = location } : null,
            IncomeLines = [new CreateIncomeLineModel
            {
                Description = GetLocation(),
                Amount = _amount
            }],
            IncomeDeductions = []
        },
        Destinations = [CreateDestination()]
    };

    private CreateIncomeTransactionDestinationModel CreateDestination()
    {
        (AccountHandle account, FundHandle fund) = GetDestination();
        return new CreateIncomeTransactionDestinationModel
        {
            AccountId = account.Id,
            Amount = _amount,
            FundAssignments = [new CreateIncomeFundAmountModel
            {
                FundId = fund.Id,
                Amount = _amount
            }]
        };
    }

    private UpdateIncomeTransactionDestinationModel CreateUpdateDestination()
    {
        (AccountHandle account, FundHandle fund) = GetDestination();
        return new UpdateIncomeTransactionDestinationModel
        {
            AccountId = account.Id,
            Amount = _amount,
            FundAssignments = [new CreateIncomeFundAmountModel
            {
                FundId = fund.Id,
                Amount = _amount
            }]
        };
    }

    private string GetLocation() => _location ?? throw new InvalidOperationException("An income transaction must have a source location.");

    private (AccountHandle Account, FundHandle Fund) GetDestination() =>
        (_destination ?? throw new InvalidOperationException("An income transaction must have a destination account."),
        _fund ?? throw new InvalidOperationException("An income transaction must have a fund assignment."));
}
