using Models.Transactions;

namespace Tests.Support;

/// <summary>
/// Starts builders for transaction setup.
/// </summary>
internal sealed class TransactionBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for a spending transaction.
    /// </summary>
    public SpendingTransactionBuilder Spending() => new(apiClient);

    /// <summary>
    /// Starts a builder for an income transaction.
    /// </summary>
    public IncomeTransactionBuilder Income() => new(apiClient);

    /// <summary>
    /// Starts a builder for an account transaction.
    /// </summary>
    public AccountTransactionBuilder Account() => new(apiClient);

    /// <summary>
    /// Starts a builder for a fund transaction.
    /// </summary>
    public FundTransactionBuilder Fund() => new(apiClient);

    /// <summary>
    /// Starts a builder for a refund transaction.
    /// </summary>
    public RefundTransactionBuilder Refund() => new(apiClient);

    /// <summary>
    /// Posts a transaction to an affected account.
    /// </summary>
    public Task PostAsync(TransactionHandle transaction, AccountHandle account, DateOnly date) => apiClient.PostAsync(
        $"/transactions/{transaction.Id}/post",
        new PostTransactionModel
        {
            AccountId = account.Id,
            Date = date
        });

    /// <summary>
    /// Unposts a transaction from the provided account, or from every affected account when none is provided.
    /// </summary>
    public Task UnpostAsync(TransactionHandle transaction, AccountHandle? account = null) => apiClient.PostAsync(
        account == null ? $"/transactions/{transaction.Id}/unpost" : $"/transactions/{transaction.Id}/unpost?accountId={account.Id}");

    /// <summary>
    /// Deletes an unposted transaction.
    /// </summary>
    public Task DeleteAsync(TransactionHandle transaction) => apiClient.DeleteAsync($"/transactions/{transaction.Id}");
}
