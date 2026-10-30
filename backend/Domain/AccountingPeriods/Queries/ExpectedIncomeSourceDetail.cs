namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents everything shown for a single Expected Income Source: the Accounting Period that owns it, the source,
/// and the Income Transactions linked to it.
/// </summary>
public sealed record ExpectedIncomeSourceDetail(
    AccountingPeriod AccountingPeriod,
    ExpectedIncomeSource Source,
    IReadOnlyCollection<ExpectedIncomeSourceIncome> Income)
{
    /// <summary>
    /// Gets the posted income received from the source across its linked Income Transactions.
    /// </summary>
    public ReceivedIncome Received => new(
        Income.Sum(income => income.Received.Total),
        Income.Sum(income => income.Received.Tracked));
}

/// <summary>
/// An Income Transaction linked to an Expected Income Source. Amount is what the transaction sends to its destination
/// Accounts, and Received is the part of it that has posted.
/// </summary>
public sealed record ExpectedIncomeSourceIncome(
    Guid TransactionId,
    DateOnly Date,
    string Description,
    decimal Amount,
    ReceivedIncome Received);
