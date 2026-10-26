using Models.Funds;
using Models.Locations;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds a refund from one or more locations or accounts to one account and fund.
/// </summary>
internal sealed class RefundTransactionBuilder(TestApiClient apiClient) : TransactionBuilder<RefundTransactionBuilder>(apiClient)
{
    private readonly List<(string? Location, AccountHandle? Account, decimal? Amount)> _sources = [];
    private AccountHandle? _destination;
    private FundHandle? _fund;

    /// <summary>
    /// Adds a source location, which refunds the transaction amount unless given its own amount.
    /// </summary>
    public RefundTransactionBuilder From(string location, decimal? amount = null)
    {
        _sources.Add((location, null, amount));
        return this;
    }

    /// <summary>
    /// Adds a source account, which refunds the transaction amount unless given its own amount.
    /// </summary>
    public RefundTransactionBuilder From(AccountHandle account, decimal? amount = null)
    {
        _sources.Add((null, account, amount));
        return this;
    }

    /// <summary>
    /// Sets the destination account and the fund the refund is assigned to.
    /// </summary>
    public RefundTransactionBuilder To(AccountHandle account, FundHandle fund)
    {
        _destination = account;
        _fund = fund;
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateRefundTransactionModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Sources = CreateSources(),
        Destination = CreateDestination()
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateRefundTransactionModel
    {
        Date = Date,
        Description = GetDescription(),
        Amount = Amount,
        Sources = CreateSources(),
        Destination = CreateDestination()
    };

    private string GetDescription()
    {
        (string? location, AccountHandle? account, _) = _sources.Count > 0
            ? _sources[0]
            : throw new InvalidOperationException("A refund must have a source.");
        return location ?? account!.Name;
    }

    private List<CreateRefundTransactionSourceModel> CreateSources()
    {
        FundHandle fund = _fund ?? throw new InvalidOperationException("A refund must have a fund assignment.");
        return _sources
            .Select(source => new CreateRefundTransactionSourceModel
            {
                AccountId = source.Account?.Id,
                Location = source.Location is null ? null : new LocationInputModel { NewLocationName = source.Location },
                Amount = source.Amount ?? Amount,
                FundAssignments = [new CreateFundAmountModel { FundId = fund.Id, Amount = source.Amount ?? Amount }]
            })
            .ToList();
    }

    private CreateRefundTransactionDestinationModel CreateDestination() => new()
    {
        AccountId = (_destination ?? throw new InvalidOperationException("A refund must have a destination account.")).Id
    };
}
