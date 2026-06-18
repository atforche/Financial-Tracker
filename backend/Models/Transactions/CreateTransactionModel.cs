using System.Text.Json.Serialization;
using Models.Funds;

namespace Models.Transactions;

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

/// <summary>
/// Model representing a request to create a spending transaction.
/// </summary>
public sealed class CreateSpendingTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Debit account for the spending transaction.
    /// </summary>
    public required Guid DebitAccountId { get; init; }

    /// <summary>
    /// Optional credit account for the spending transaction.
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <summary>
    /// Optional destination location for the spending transaction.
    /// </summary>
    public string? DestinationLocation { get; init; }

    /// <summary>
    /// Fund assignments for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a request to create an income transaction.
/// </summary>
public sealed class CreateIncomeTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Optional debit account for the income transaction.
    /// </summary>
    public Guid? DebitAccountId { get; init; }

    /// <summary>
    /// Optional source location for the income transaction.
    /// </summary>
    public string? SourceLocation { get; init; }

    /// <summary>
    /// Credit account for the income transaction.
    /// </summary>
    public required Guid CreditAccountId { get; init; }

    /// <summary>
    /// Fund assignments for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a request to create an account transaction.
/// </summary>
public sealed class CreateAccountTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Optional debit account for the account transaction.
    /// </summary>
    public Guid? DebitAccountId { get; init; }

    /// <summary>
    /// Optional credit account for the account transaction.
    /// </summary>
    public Guid? CreditAccountId { get; init; }
}

/// <summary>
/// Model representing a request to create a fund transaction.
/// </summary>
public sealed class CreateFundTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Debit fund for the fund transaction.
    /// </summary>
    public required Guid DebitFundId { get; init; }

    /// <summary>
    /// Credit fund for the fund transaction.
    /// </summary>
    public required Guid CreditFundId { get; init; }
}