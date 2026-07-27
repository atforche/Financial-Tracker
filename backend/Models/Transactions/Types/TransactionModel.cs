using System.Text.Json.Serialization;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a Transaction.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SpendingTransactionModel), nameof(TransactionTypeModel.Spending))]
[JsonDerivedType(typeof(IncomeTransactionModel), nameof(TransactionTypeModel.Income))]
[JsonDerivedType(typeof(AccountTransactionModel), nameof(TransactionTypeModel.Account))]
[JsonDerivedType(typeof(FundTransactionModel), nameof(TransactionTypeModel.Fund))]
public abstract class TransactionModel
{
    /// <summary>
    /// ID for the Transaction.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Type of the Transaction.
    /// </summary>
    public required TransactionTypeModel TransactionType { get; init; }

    /// <summary>
    /// Accounting Period ID for the Transaction.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Name of the Accounting Period for the Transaction.
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Date for the Transaction.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for the Transaction.
    /// </summary>
    public required int Sequence { get; init; }

    /// <summary>
    /// Description for the Transaction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Whether the Transaction is posted to every affected Account.
    /// </summary>
    public required bool FullyPosted { get; init; }
}