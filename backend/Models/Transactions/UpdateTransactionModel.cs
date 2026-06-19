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
/// Model representing a request to update a spending transaction.
/// </summary>
public sealed class UpdateSpendingTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Fund assignments for the spending transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
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
/// Model representing an income destination in an update income transaction request.
/// </summary>
public sealed class UpdateIncomeDestinationModel
{
    /// <summary>
    /// Destination account for the income transaction.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Amount directed to the destination account.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fund assignments for the destination account.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}

/// <summary>
/// Model representing a request to update an income transaction.
/// </summary>
public sealed class UpdateIncomeTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Income lines for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeDeductionModel> IncomeDeductions { get; init; }

    /// <summary>
    /// Income destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeDestinationModel> IncomeDestinations { get; init; }
}

/// <summary>
/// Model representing a request to update an account transaction.
/// </summary>
public sealed class UpdateAccountTransactionModel : UpdateTransactionModel
{
}

/// <summary>
/// Model representing a request to update a fund transaction.
/// </summary>
public sealed class UpdateFundTransactionModel : UpdateTransactionModel
{
}
