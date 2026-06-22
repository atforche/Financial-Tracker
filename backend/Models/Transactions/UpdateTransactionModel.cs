using System.Text.Json.Serialization;
using Models.Funds;

namespace Models.Transactions;

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

/// <summary>
/// Model representing the source of a spending transaction update request.
/// </summary>
public sealed class UpdateSpendingTransactionSourceModel
{
    /// <summary>
    /// Account ID for the source account.
    /// </summary>
    public required Guid AccountId { get; init; }
}

/// <summary>
/// Model representing a destination of a spending transaction update request.
/// </summary>
public sealed class UpdateSpendingTransactionDestinationModel
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
/// Model representing a request to update a spending transaction.
/// </summary>
public sealed class UpdateSpendingTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the spending transaction.
    /// </summary>
    public required UpdateSpendingTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateSpendingTransactionDestinationModel> Destinations { get; init; }

    /// <summary>
    /// Flattened view of fund assignments across all destinations.
    /// </summary>
    public IReadOnlyCollection<CreateFundAmountModel> FundAssignments => Destinations.SelectMany(destination => destination.FundAssignments).ToList();
}

/// <summary>
/// Model representing an income line in an update income transaction request.
/// </summary>
public sealed class UpdateIncomeLineModel
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
/// Model representing an income deduction in an update income transaction request.
/// </summary>
public sealed class UpdateIncomeDeductionModel
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
/// Model representing the source of an income transaction update request.
/// </summary>
public sealed class UpdateIncomeTransactionSourceModel
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
    public required IReadOnlyCollection<UpdateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeDeductionModel> IncomeDeductions { get; init; }
}

/// <summary>
/// Model representing a destination of an income transaction update request.
/// </summary>
public sealed class UpdateIncomeTransactionDestinationModel
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
/// Model representing a request to update an income transaction.
/// </summary>
public sealed class UpdateIncomeTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required UpdateIncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeTransactionDestinationModel> Destinations { get; init; }

    /// <summary>
    /// Backwards-compatible alias for the income destinations collection.
    /// </summary>
    public IReadOnlyCollection<UpdateIncomeTransactionDestinationModel> IncomeDestinations => Destinations;
}

/// <summary>
/// Model representing the source of an account transaction update request.
/// </summary>
public sealed class UpdateAccountTransactionSourceModel
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
/// Model representing a destination of an account transaction update request.
/// </summary>
public sealed class UpdateAccountTransactionDestinationModel
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
/// Model representing a request to update an account transaction.
/// </summary>
public sealed class UpdateAccountTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the account transaction.
    /// </summary>
    public required UpdateAccountTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the account transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateAccountTransactionDestinationModel> Destinations { get; init; }
}

/// <summary>
/// Model representing the source of a fund transaction update request.
/// </summary>
public sealed class UpdateFundTransactionSourceModel
{
    /// <summary>
    /// Fund ID for the source fund.
    /// </summary>
    public required Guid FundId { get; init; }
}

/// <summary>
/// Model representing a destination of a fund transaction update request.
/// </summary>
public sealed class UpdateFundTransactionDestinationModel
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
/// Model representing a request to update a fund transaction.
/// </summary>
public sealed class UpdateFundTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the fund transaction.
    /// </summary>
    public required UpdateFundTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the fund transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateFundTransactionDestinationModel> Destinations { get; init; }
}