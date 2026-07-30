using System.Text.Json.Serialization;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update a Transaction.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UpdateSpendingTransactionModel), nameof(TransactionTypeModel.Spending))]
[JsonDerivedType(typeof(UpdateIncomeTransactionModel), nameof(TransactionTypeModel.Income))]
[JsonDerivedType(typeof(UpdateAccountTransactionModel), nameof(TransactionTypeModel.Account))]
[JsonDerivedType(typeof(UpdateFundTransactionModel), nameof(TransactionTypeModel.Fund))]
public abstract class UpdateTransactionModel
{
    /// <summary>
    /// Date for the Transaction.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Description for the Transaction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction.
    /// </summary>
    public required decimal Amount { get; init; }
}