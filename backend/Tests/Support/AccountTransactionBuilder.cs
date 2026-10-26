using Models.Locations;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds an account transfer from an account or location to one or more accounts.
/// </summary>
internal sealed class AccountTransactionBuilder(TestApiClient apiClient) : TransactionBuilder<AccountTransactionBuilder>(apiClient)
{
    private readonly List<(AccountHandle Account, decimal? Amount)> _destinations = [];
    private AccountHandle? _sourceAccount;
    private string? _sourceLocation;

    /// <summary>
    /// Sets the source account.
    /// </summary>
    public AccountTransactionBuilder From(AccountHandle account)
    {
        _sourceAccount = account;
        return this;
    }

    /// <summary>
    /// Sets an untracked source location.
    /// </summary>
    public AccountTransactionBuilder From(string location)
    {
        _sourceLocation = location;
        return this;
    }

    /// <summary>
    /// Adds a destination account, which receives the transaction amount unless given its own amount.
    /// </summary>
    public AccountTransactionBuilder To(AccountHandle account, decimal? amount = null)
    {
        _destinations.Add((account, amount));
        return this;
    }

    /// <inheritdoc/>
    public override CreateTransactionModel CreateRequest() => new CreateAccountTransactionModel
    {
        AccountingPeriodId = PeriodId,
        Date = Date,
        Description = "Transfer",
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    /// <inheritdoc/>
    public override UpdateTransactionModel UpdateRequest() => new UpdateAccountTransactionModel
    {
        Date = Date,
        Description = "Transfer",
        Amount = Amount,
        Source = CreateSource(),
        Destinations = CreateDestinations()
    };

    private CreateAccountTransactionSourceModel CreateSource() => new()
    {
        AccountId = _sourceAccount?.Id,
        Location = _sourceLocation is null ? null : new LocationInputModel { NewLocationName = _sourceLocation }
    };

    private List<CreateAccountTransactionDestinationModel> CreateDestinations() => _destinations
        .Select(destination => new CreateAccountTransactionDestinationModel
        {
            AccountId = destination.Account.Id,
            Amount = destination.Amount ?? Amount
        })
        .ToList();
}
