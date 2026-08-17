using Models.Transactions.Types;

namespace Models.Transactions;

/// <summary>
/// Model representing the collection of Transactions within a specified accounting period range.
/// </summary>
public class TransactionsInAccountingPeriodRangeModel : PaginationModel
{
    /// <summary>
    /// Transactions for the requested accounting period range.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Available Account Names for the current snapshot filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Available Fund Names for the current snapshot filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Transaction Type in the current Accounting Period.
    /// </summary>
    public required IReadOnlyCollection<TransactionSummaryByTypeModel> TransactionTypes { get; init; }

    /// <summary>
    /// Money received by the selected Locations in the current Accounting Period range.
    /// </summary>
    public required decimal LocationIncomingAmount { get; init; }

    /// <summary>
    /// Money sent to the selected Locations in the current Accounting Period range.
    /// </summary>
    public required decimal LocationOutgoingAmount { get; init; }
}