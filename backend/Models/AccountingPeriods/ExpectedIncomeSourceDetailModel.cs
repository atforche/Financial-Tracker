namespace Models.AccountingPeriods;

/// <summary>
/// Model representing everything shown for a single Expected Income Source.
/// </summary>
public class ExpectedIncomeSourceDetailModel
{
    /// <summary>
    /// The Accounting Period that owns the source.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// The source, with the income received from it so far.
    /// </summary>
    public required ExpectedIncomeSourceModel Source { get; init; }

    /// <summary>
    /// The Income Transactions linked to the source, in date order.
    /// </summary>
    public required IReadOnlyCollection<ExpectedIncomeSourceIncomeModel> Income { get; init; }
}

/// <summary>
/// Model representing an Income Transaction linked to an Expected Income Source.
/// </summary>
public class ExpectedIncomeSourceIncomeModel
{
    /// <summary>
    /// Identifier for the Income Transaction.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Date of the Income Transaction.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Description of the Income Transaction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount the Income Transaction sends to its destination Accounts.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Part of the amount that has posted.
    /// </summary>
    public required IncomeAmountModel ReceivedAmount { get; init; }
}
