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

    /// <summary>
    /// Optional destination location for the transaction.
    /// </summary>
    public string? DestinationLocation { get; init; }

    /// <summary>
    /// Fund assignments for the transaction.
    /// </summary>
    public required IReadOnlyCollection<TransactionFundModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing an income line on an income transaction.
/// </summary>
public sealed class IncomeLineModel
{
    /// <summary>
    /// Description for the income line.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the income line.
    /// </summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Model representing an income deduction on an income transaction.
/// </summary>
public sealed class IncomeDeductionModel
{
    /// <summary>
    /// Description for the income deduction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the income deduction.
    /// </summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Model representing an income destination on an income transaction.
/// </summary>
public sealed class IncomeDestinationModel
{
    /// <summary>
    /// Destination account for the income transaction.
    /// </summary>
    public required TransactionAccountModel Account { get; init; }

    /// <summary>
    /// Amount directed to the destination account.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for the destination account.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for the destination account.
    /// </summary>
    public required IReadOnlyCollection<TransactionFundModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing an income transaction.
/// </summary>
public sealed class IncomeTransactionModel : TransactionModel
{
    /// <summary>
    /// Optional source account for the transaction.
    /// </summary>
    public TransactionAccountModel? SourceAccount { get; init; }

    /// <summary>
    /// Optional source location for the transaction.
    /// </summary>
    public string? SourceLocation { get; init; }

    /// <summary>
    /// Total tracked income amount for the transaction.
    /// </summary>
    public required decimal TrackedIncomeAmount { get; init; }

    /// <summary>
    /// Income lines for the transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeductionModel> IncomeDeductions { get; init; }

    /// <summary>
    /// Income destinations for the transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeDestinationModel> IncomeDestinations { get; init; }
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
