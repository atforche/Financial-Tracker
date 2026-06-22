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
/// Model representing the source of a spending transaction create request.
/// </summary>
public sealed class CreateSpendingTransactionSourceModel
{
    /// <summary>
    /// Account ID for the source account.
    /// </summary>
    public required Guid AccountId { get; init; }
}

/// <summary>
/// Model representing a destination of a spending transaction create request.
/// </summary>
public sealed class CreateSpendingTransactionDestinationModel
{
    /// <summary>
    /// Optional account ID for the destination account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a request to create a spending transaction.
/// </summary>
public sealed class CreateSpendingTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the spending transaction.
    /// </summary>
    public required CreateSpendingTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateSpendingTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing an income line in a create income transaction request.
/// </summary>
public sealed class CreateIncomeLineModel
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
/// Model representing an income deduction in a create income transaction request.
/// </summary>
public sealed class CreateIncomeDeductionModel
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
/// Model representing the source of an income transaction create request.
/// </summary>
public sealed class CreateIncomeTransactionSourceModel
{
    /// <summary>
    /// Optional account ID for the income source.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the income source.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Income lines for the source.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeDeductionModel> IncomeDeductions { get; init; }
}

/// <summary>
/// Model representing a destination of an income transaction create request.
/// </summary>
public sealed class CreateIncomeTransactionDestinationModel
{
    /// <summary>
    /// Account ID for the destination account.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a request to create an income transaction.
/// </summary>
public sealed class CreateIncomeTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required CreateIncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing the source of an account transaction create request.
/// </summary>
public sealed class CreateAccountTransactionSourceModel
{
    /// <summary>
    /// Optional account ID for the source account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }
}

/// <summary>
/// Model representing a destination of an account transaction create request.
/// </summary>
public sealed class CreateAccountTransactionDestinationModel
{
    /// <summary>
    /// Optional account ID for the destination account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Model representing a request to create an account transaction.
/// </summary>
public sealed class CreateAccountTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required CreateAccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateAccountTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing the source of a fund transaction create request.
/// </summary>
public sealed class CreateFundTransactionSourceModel
{
    /// <summary>
    /// Fund ID for the source fund.
    /// </summary>
    public required Guid FundId { get; init; }
}

/// <summary>
/// Model representing a destination of a fund transaction create request.
/// </summary>
public sealed class CreateFundTransactionDestinationModel
{
    /// <summary>
    /// Fund ID for the destination fund.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Amount directed to this destination fund.
    /// </summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Model representing a request to create a fund transaction.
/// </summary>
public sealed class CreateFundTransactionModel : CreateTransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required CreateFundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundTransactionDestinationModel> Destinations { get; init; }
}