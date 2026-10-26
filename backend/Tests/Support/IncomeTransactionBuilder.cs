using Models.Funds;
using Models.Locations;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds an income transaction with one income line from a location or account to one or more accounts.
/// </summary>
internal sealed class IncomeTransactionBuilder(TestApiClient apiClient) : TransactionBuilder<IncomeTransactionBuilder>(apiClient)
{
    private readonly List<(AccountHandle Account, FundHandle? Fund, decimal? Amount)> _destinations = [];
    private string? _sourceLocation;
    private AccountHandle? _sourceAccount;

    /// <summary>
    /// Sets the income source location.
    /// </summary>
    public IncomeTransactionBuilder From(string location)
    {
        _sourceLocation = location;
        return this;
    }

    /// <summary>
    /// Sets the income source account.
    /// </summary>
    public IncomeTransactionBuilder From(AccountHandle account)
    {
        _sourceAccount = account;
        return this;
    }

    /// <summary>
    /// Adds a destination account and the fund it assigns its whole amount to, or no fund assignment when the fund is null.
    /// The destination receives the transaction amount unless given its own amount.
    /// </summary>
    public IncomeTransactionBuilder To(AccountHandle account, FundHandle? fund, decimal? amount = null)
    {
        _destinations.Add((account, fund, amount));
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateIncomeTransactionModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateIncomeTransactionModel
    {
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    private string GetDescription() => _sourceLocation
        ?? _sourceAccount?.Name
        ?? throw new InvalidOperationException("An income transaction must have a source.");

    private CreateIncomeTransactionSourceModel CreateSource() => new()
    {
        AccountId = _sourceAccount?.Id,
        Location = _sourceLocation is null ? null : new LocationInputModel { NewLocationName = _sourceLocation },
        IncomeLines = [new CreateIncomeLineModel { Description = GetDescription(), Amount = Amount }],
        IncomeDeductions = []
    };

    private List<CreateIncomeTransactionDestinationModel> CreateDestinations() => _destinations
        .Select(destination => new CreateIncomeTransactionDestinationModel
        {
            AccountId = destination.Account.Id,
            Amount = destination.Amount ?? Amount,
            FundAssignments = destination.Fund is null
                ? []
                : [new CreateIncomeFundAmountModel { FundId = destination.Fund.Id, Amount = destination.Amount ?? Amount }]
        })
        .ToList();
}
