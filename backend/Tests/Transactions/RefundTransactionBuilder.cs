using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>Builds a refund from a location into a tracked account and fund.</summary>
internal sealed class RefundTransactionBuilder(TestApiClient apiClient)
{
    private AccountingPeriodHandle? _period;
    private DateOnly _date;
    private decimal _amount;
    private string? _location;
    private AccountHandle? _sourceAccount;
    private AccountHandle? _destination;
    private FundHandle? _fund;
    private readonly List<(string? Location, AccountHandle? Account, decimal Amount)> _sources = [];

    public RefundTransactionBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }
    public RefundTransactionBuilder On(DateOnly date)
    {
        _date = date;
        return this;
    }
    public RefundTransactionBuilder For(decimal amount)
    {
        _amount = amount;
        return this;
    }
    public RefundTransactionBuilder From(string location)
    {
        _location = location;
        return this;
    }
    public RefundTransactionBuilder From(string location, decimal amount)
    {
        _sources.Add((location, null, amount));
        return this;
    }
    public RefundTransactionBuilder From(AccountHandle account)
    {
        _sourceAccount = account;
        return this;
    }
    public RefundTransactionBuilder From(AccountHandle account, decimal amount)
    {
        _sources.Add((null, account, amount));
        return this;
    }
    public RefundTransactionBuilder To(AccountHandle account, FundHandle fund)
    {
        _destination = account;
        _fund = fund;
        return this;
    }

    public async Task<TransactionHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.");
        AccountHandle account = _destination ?? throw new InvalidOperationException("A refund must have a destination account.");
        FundHandle fund = _fund ?? throw new InvalidOperationException("A refund must have a fund assignment.");
        if (_sources.Count == 0 && _location == null && _sourceAccount == null)
        {
            throw new InvalidOperationException("A refund must have a source.");
        }
        IReadOnlyCollection<(string? Location, AccountHandle? Account, decimal Amount)> sources = _sources.Count > 0
            ? _sources
            : [(_location, _sourceAccount, _amount)];
        string description = sources.First().Location ?? sources.First().Account!.Name;
        CreateTransactionResultModel result = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateRefundTransactionModel
        {
            AccountingPeriodId = period.Id,
            Date = _date,
            Description = description,
            Amount = _amount,
            Sources = sources.Select(source => new CreateRefundTransactionSourceModel
            {
                AccountId = source.Account?.Id,
                Location = source.Location == null ? null : new LocationInputModel { NewLocationName = source.Location },
                Amount = source.Amount,
                FundAssignments = [new CreateFundAmountModel { FundId = fund.Id, Amount = source.Amount }]
            }).ToList(),
            Destination = new CreateRefundTransactionDestinationModel { AccountId = account.Id }
        });
        return new TransactionHandle(result.Id);
    }
}
