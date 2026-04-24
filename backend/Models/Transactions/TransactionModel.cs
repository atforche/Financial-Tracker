using System.Text.Json.Serialization;

namespace Models.Transactions;

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
    /// Location for the Transaction.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Description for the Transaction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction.
    /// </summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Model representing a spending transaction.
/// </summary>
public sealed class SpendingTransactionModel : TransactionModel
{
    /// <summary>
    /// Debit account for the transaction.
    /// </summary>
    public required TransactionAccountModel DebitAccount { get; init; }

    /// <summary>
    /// Optional credit account for the transaction.
    /// </summary>
    public TransactionAccountModel? CreditAccount { get; init; }
}

/// <summary>
/// Model representing an income transaction.
/// </summary>
public sealed class IncomeTransactionModel : TransactionModel
{
    /// <summary>
    /// Optional debit account for the transaction.
    /// </summary>
    public TransactionAccountModel? DebitAccount { get; init; }

    /// <summary>
    /// Credit account for the transaction.
    /// </summary>
    public required TransactionAccountModel CreditAccount { get; init; }
}

/// <summary>
/// Model representing an account transaction.
/// </summary>
public sealed class AccountTransactionModel : TransactionModel
{
    /// <summary>
    /// Optional debit account for the transaction.
    /// </summary>
    public TransactionAccountModel? DebitAccount { get; init; }

    /// <summary>
    /// Optional credit account for the transaction.
    /// </summary>
    public TransactionAccountModel? CreditAccount { get; init; }
}

/// <summary>
/// Model representing a fund transaction.
/// </summary>
public sealed class FundTransactionModel : TransactionModel
{
    /// <summary>
    /// Debit fund for the transaction.
    /// </summary>
    public required TransactionFundModel DebitFund { get; init; }

    /// <summary>
    /// Credit fund for the transaction.
    /// </summary>
    public required TransactionFundModel CreditFund { get; init; }
}