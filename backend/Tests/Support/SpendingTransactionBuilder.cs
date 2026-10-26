using Models.Funds;
using Models.Locations;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds a spending transaction from one account to one or more locations or accounts, each assigned to a fund.
/// </summary>
internal sealed class SpendingTransactionBuilder(TestApiClient apiClient) : TransactionBuilder<SpendingTransactionBuilder>(apiClient)
{
    private readonly List<(string? Location, AccountHandle? Account, FundHandle Fund, decimal? Amount)> _destinations = [];
    private AccountHandle? _source;

    /// <summary>
    /// Sets the source account.
    /// </summary>
    public SpendingTransactionBuilder From(AccountHandle account)
    {
        _source = account;
        return this;
    }

    /// <summary>
    /// Adds a destination location and its fund, which receive the transaction amount unless given their own amount.
    /// </summary>
    public SpendingTransactionBuilder To(string location, FundHandle fund, decimal? amount = null)
    {
        _destinations.Add((location, null, fund, amount));
        return this;
    }

    /// <summary>
    /// Adds a destination account and its fund, which receive the transaction amount unless given their own amount.
    /// </summary>
    public SpendingTransactionBuilder To(AccountHandle account, FundHandle fund, decimal? amount = null)
    {
        _destinations.Add((null, account, fund, amount));
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateSpendingTransactionModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateSpendingTransactionModel
    {
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    private string GetDescription() => _destinations.FirstOrDefault().Location ?? "Spending";

    private CreateSpendingTransactionSourceModel CreateSource() => new()
    {
        AccountId = (_source ?? throw new InvalidOperationException("A spending transaction must have a source account.")).Id
    };

    private List<CreateSpendingTransactionDestinationModel> CreateDestinations() => _destinations
        .Select(destination => new CreateSpendingTransactionDestinationModel
        {
            AccountId = destination.Account?.Id,
            Location = destination.Location is null ? null : new LocationInputModel { NewLocationName = destination.Location },
            Amount = destination.Amount ?? Amount,
            FundAssignments = [new CreateFundAmountModel { FundId = destination.Fund.Id, Amount = destination.Amount ?? Amount }]
        })
        .ToList();
}
