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
/// Model representing the source of a spending transaction response.
/// </summary>
public sealed class SpendingTransactionSourceModel
{
    /// <summary>
    /// Account for the source.
    /// </summary>
    public required TransactionAccountModel Account { get; init; }
}

/// <summary>
/// Model representing a destination of a spending transaction response.
/// </summary>
public sealed class SpendingTransactionDestinationModel
{
    /// <summary>
    /// Optional account for the destination.
    /// </summary>
    public TransactionAccountModel? Account { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this destination.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<TransactionFundModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a spending transaction.
/// </summary>
public sealed class SpendingTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the spending transaction.
    /// </summary>
    public required SpendingTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<SpendingTransactionDestinationModel> Destinations { get; init; }
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
/// Model representing the source of an income transaction response.
/// </summary>
public sealed class IncomeTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public TransactionAccountModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Income lines for the source.
    /// </summary>
    public required IReadOnlyCollection<IncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeductionModel> IncomeDeductions { get; init; }
}

/// <summary>
/// Model representing a destination of an income transaction response.
/// </summary>
public sealed class IncomeTransactionDestinationModel
{
    /// <summary>
    /// Account for the destination.
    /// </summary>
    public required TransactionAccountModel Account { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this destination.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<TransactionFundModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing an income transaction.
/// </summary>
public sealed class IncomeTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required IncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Total tracked amount for the income transaction.
    /// </summary>
    public required decimal TrackedAmount { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing the source of an account transaction response.
/// </summary>
public sealed class AccountTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public TransactionAccountModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }
}

/// <summary>
/// Model representing a destination of an account transaction response.
/// </summary>
public sealed class AccountTransactionDestinationModel
{
    /// <summary>
    /// Optional account for the destination.
    /// </summary>
    public TransactionAccountModel? Account { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this destination.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }
}

/// <summary>
/// Model representing an account transaction.
/// </summary>
public sealed class AccountTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required AccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<AccountTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing the source of a fund transaction response.
/// </summary>
public sealed class FundTransactionSourceModel
{
    /// <summary>
    /// Fund for the source.
    /// </summary>
    public required TransactionFundModel Fund { get; init; }
}

/// <summary>
/// Model representing a destination of a fund transaction response.
/// </summary>
public sealed class FundTransactionDestinationModel
{
    /// <summary>
    /// Fund for the destination.
    /// </summary>
    public required TransactionFundModel Fund { get; init; }
}

/// <summary>
/// Model representing a fund transaction.
/// </summary>
public sealed class FundTransactionModel : TransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required FundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<FundTransactionDestinationModel> Destinations { get; init; }
}