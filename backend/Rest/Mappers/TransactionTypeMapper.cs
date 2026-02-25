using System.Diagnostics.CodeAnalysis;
using Data.Transactions;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transaction Types to Transaction Type Models
/// </summary>
internal sealed class TransactionTypeMapper
{
    /// <summary>
    /// Attempts to map the provided Transaction Type Model to a Transaction Type
    /// </summary>
    public static bool TryToData(TransactionTypeModel transactionTypeModel, [NotNullWhen(true)] out TransactionType? transactionType)
    {
        transactionType = transactionTypeModel switch
        {
            TransactionTypeModel.Debit => TransactionType.Debit,
            TransactionTypeModel.Credit => TransactionType.Credit,
            TransactionTypeModel.Transfer => TransactionType.Transfer,
            _ => null
        };
        return transactionType != null;
    }
}