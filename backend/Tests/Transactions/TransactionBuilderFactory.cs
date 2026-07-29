using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Starts builders for transaction setup.
/// </summary>
internal sealed class TransactionBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for a spending transaction.
    /// </summary>
    public SpendingTransactionBuilder Spending() => new(apiClient);
}