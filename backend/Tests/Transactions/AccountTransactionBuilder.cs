using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Builds an account-to-account transfer transaction.
/// </summary>
internal sealed class AccountTransactionBuilder(TestApiClient apiClient)
{
    private AccountingPeriodHandle? _period;
    private DateOnly _date;
    private decimal _amount;
    private AccountHandle? _source;
    private AccountHandle? _destination;

    /// <summary>
    /// Sets the accounting period for the transaction.
    /// </summary>
    public AccountTransactionBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Sets the transaction date.
    /// </summary>
    public AccountTransactionBuilder On(DateOnly date)
    {
        _date = date;
        return this;
    }

    /// <summary>
    /// Sets the transfer amount.
    /// </summary>
    public AccountTransactionBuilder For(decimal amount)
    {
        _amount = amount;
        return this;
    }

    /// <summary>
    /// Sets the source account.
    /// </summary>
    public AccountTransactionBuilder From(AccountHandle account)
    {
        _source = account;
        return this;
    }

    /// <summary>
    /// Sets the destination account.
    /// </summary>
    public AccountTransactionBuilder To(AccountHandle account)
    {
        _destination = account;
        return this;
    }

    /// <summary>
    /// Creates the transaction.
    /// </summary>
    public async Task<TransactionHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.");
        CreateTransactionModel request = new CreateAccountTransactionModel
        {
            AccountingPeriodId = period.Id,
            Date = _date,
            Description = "Transfer",
            Amount = _amount,
            Source = new CreateAccountTransactionSourceModel
            {
                AccountId = GetSource().Id
            },
            Destinations = [new CreateAccountTransactionDestinationModel
            {
                AccountId = GetDestination().Id,
                Amount = _amount
            }]
        };
        CreateTransactionResultModel model = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        return new TransactionHandle(model.Id);
    }

    /// <summary>
    /// Updates an unposted transfer transaction with this builder's values.
    /// </summary>
    public Task UpdateAsync(TransactionHandle transaction)
    {
        UpdateTransactionModel request = new UpdateAccountTransactionModel
        {
            Date = _date,
            Description = "Transfer",
            Amount = _amount,
            Source = new UpdateAccountTransactionSourceModel
            {
                AccountId = GetSource().Id
            },
            Destinations = [new UpdateAccountTransactionDestinationModel
            {
                AccountId = GetDestination().Id,
                Amount = _amount
            }]
        };
        return apiClient.PostAsync($"/transactions/{transaction.Id}", request);
    }

    private AccountHandle GetSource() => _source ?? throw new InvalidOperationException("An account transaction must have a source account.");

    private AccountHandle GetDestination() => _destination ?? throw new InvalidOperationException("An account transaction must have a destination account.");
}
