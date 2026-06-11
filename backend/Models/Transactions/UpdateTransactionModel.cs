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
/// Model representing a request to update an income transaction.
/// </summary>
public sealed class UpdateIncomeTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Fund assignments for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
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