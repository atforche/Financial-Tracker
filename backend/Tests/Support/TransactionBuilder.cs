using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Tests.Support;

/// <summary>
/// Builds a transaction whose create and update requests share the same values.
/// </summary>
internal abstract class TransactionBuilder<TBuilder>(TestApiClient apiClient)
    where TBuilder : TransactionBuilder<TBuilder>
{
    private AccountingPeriodHandle? _period;

    /// <summary>
    /// Gets the transaction date.
    /// </summary>
    protected DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    protected decimal Amount { get; private set; }

    /// <summary>
    /// Gets the ID of the transaction's accounting period.
    /// </summary>
    protected Guid PeriodId => (_period ?? throw new InvalidOperationException("A transaction must belong to an accounting period.")).Id;

    /// <summary>
    /// Sets the accounting period for the transaction.
    /// </summary>
    public TBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the transaction date.
    /// </summary>
    public TBuilder On(DateOnly date)
    {
        Date = date;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the transaction amount, which single-destination parties receive unless given their own amount.
    /// </summary>
    public TBuilder For(decimal amount)
    {
        Amount = amount;
        return (TBuilder)this;
    }

    /// <summary>
    /// Builds the request that creates the transaction.
    /// </summary>
    public abstract CreateTransactionModel CreateRequest();

    /// <summary>
    /// Builds the request that updates an unposted transaction to this builder's values.
    /// </summary>
    public abstract UpdateTransactionModel UpdateRequest();

    /// <summary>
    /// Creates the transaction.
    /// </summary>
    public async Task<TransactionHandle> CreateAsync()
    {
        CreateTransactionResultModel model = await apiClient.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", CreateRequest());
        return new TransactionHandle(model.Id);
    }

    /// <summary>
    /// Updates an unposted transaction with this builder's values.
    /// </summary>
    public Task UpdateAsync(TransactionHandle transaction) => apiClient.PostAsync($"/transactions/{transaction.Id}", UpdateRequest());
}
