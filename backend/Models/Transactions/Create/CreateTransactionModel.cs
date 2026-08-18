using System.Text.Json.Serialization;

namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create a Transaction.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreateSpendingTransactionModel), nameof(TransactionTypeModel.Spending))]
[JsonDerivedType(typeof(CreateIncomeTransactionModel), nameof(TransactionTypeModel.Income))]
[JsonDerivedType(typeof(CreateAccountTransactionModel), nameof(TransactionTypeModel.Account))]
[JsonDerivedType(typeof(CreateFundTransactionModel), nameof(TransactionTypeModel.Fund))]
public abstract class CreateTransactionModel
{
    /// <summary>
    /// Accounting Period for the Transaction.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

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
