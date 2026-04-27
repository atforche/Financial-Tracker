using Domain.Transactions;
using Models.Transactions;

namespace Rest.Transactions;

/// <summary>
/// Converter class that handles converting transaction types between domain and REST models.
/// </summary>
internal static class TransactionTypeConverter
{
    /// <summary>
    /// Converts the provided domain transaction type to a REST model transaction type.
    /// </summary>
    public static TransactionTypeModel ToModel(TransactionType transactionType) => transactionType switch
    {
        TransactionType.Spending => TransactionTypeModel.Spending,
        TransactionType.Income => TransactionTypeModel.Income,
        TransactionType.Account => TransactionTypeModel.Account,
        TransactionType.Fund => TransactionTypeModel.Fund,
        _ => throw new InvalidOperationException($"Unrecognized transaction type: {transactionType}")
    };

    /// <summary>
    /// Converts the provided update transaction model runtime type to a REST model transaction type.
    /// </summary>
    public static TransactionTypeModel ToModel(UpdateTransactionModel transactionModel) => transactionModel switch
    {
        UpdateSpendingTransactionModel => TransactionTypeModel.Spending,
        UpdateIncomeTransactionModel => TransactionTypeModel.Income,
        UpdateAccountTransactionModel => TransactionTypeModel.Account,
        UpdateFundTransactionModel => TransactionTypeModel.Fund,
        _ => throw new InvalidOperationException($"Unrecognized transaction model type: {transactionModel.GetType().Name}")
    };
}