using Models.Transactions.Types;

namespace Models.Transactions;

/// <summary>
/// Model representing the collection of Transactions within a specified date range.
/// </summary>
public class TransactionsInDateRangeModel : PaginationModel
{
    /// <summary>
    /// Transactions for the requested date range.
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
    /// Summary counts and amounts for each Transaction Type in the current date range.
    /// </summary>
    public required IReadOnlyCollection<TransactionSummaryByTypeModel> TransactionTypes { get; init; }

    /// <summary>
    /// Money received by the selected Locations in the current date range.
    /// </summary>
    public required decimal LocationIncomingAmount { get; init; }

    /// <summary>
    /// Money sent to the selected Locations in the current date range.
    /// </summary>
    public required decimal LocationOutgoingAmount { get; init; }
}