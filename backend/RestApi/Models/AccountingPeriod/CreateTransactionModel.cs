using RestApi.Models.FundAmount;

namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Transaction
/// </summary>
public class CreateTransactionModel
{
    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.TransactionDate"/>
    public required DateOnly TransactionDate { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.AccountingEntries"/>
    public required IReadOnlyCollection<CreateFundAmountModel> AccountingEntries { get; init; }

    /// <summary>
    /// Detail of the Account being debited by this Transaction
    /// </summary>
    public CreateTransactionAccountDetailModel? DebitDetail { get; init; }

    /// <summary>
    /// Details of the Account being credited by this Transaction
    /// </summary>
    public CreateTransactionAccountDetailModel? CreditDetail { get; init; }
}

/// <summary>
/// REST model representing a request to create a Transaction Account Detail
/// </summary>
public class CreateTransactionAccountDetailModel
{
    /// <summary>
    /// Account being affected by this Transaction
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Posted Statement Date for the Transaction in this Account
    /// </summary>
    public DateOnly? PostedStatementDate { get; init; }
}