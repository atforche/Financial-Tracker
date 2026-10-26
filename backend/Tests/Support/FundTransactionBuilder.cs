using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds a fund transfer from one fund to one or more funds.
/// </summary>
internal sealed class FundTransactionBuilder(TestApiClient apiClient) : TransactionBuilder<FundTransactionBuilder>(apiClient)
{
    private readonly List<(FundHandle Fund, decimal? Amount)> _destinations = [];
    private FundHandle? _source;

    /// <summary>
    /// Sets the source fund.
    /// </summary>
    public FundTransactionBuilder From(FundHandle fund)
    {
        _source = fund;
        return this;
    }

    /// <summary>
    /// Adds a destination fund, which receives the transaction amount unless given its own amount.
    /// </summary>
    public FundTransactionBuilder To(FundHandle fund, decimal? amount = null)
    {
        _destinations.Add((fund, amount));
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateFundTransactionModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = "Fund transfer",
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateFundTransactionModel
    {
        Date = Date,
        Description = "Fund transfer",
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    private CreateFundTransactionSourceModel CreateSource() => new()
    {
        FundId = (_source ?? throw new InvalidOperationException("A fund transaction must have a source fund.")).Id
    };

    private List<CreateFundTransactionDestinationModel> CreateDestinations() => _destinations
        .Select(destination => new CreateFundTransactionDestinationModel
        {
            FundId = destination.Fund.Id,
            Amount = destination.Amount ?? Amount
        })
        .ToList();
}
