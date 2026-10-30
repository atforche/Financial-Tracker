using Domain.Accounts;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents an Accounting Period and its persisted balance totals, with the posted income linked to each of its
/// Expected Income Sources.
/// </summary>
public sealed record AccountingPeriodBalance(
    AccountingPeriod AccountingPeriod,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal ActualIncome,
    decimal ActualTrackedIncome,
    decimal ActualUnplannedIncome,
    decimal ActualUnplannedTrackedIncome,
    decimal PlannedGoalContributions,
    decimal ExpectedGoalContributions,
    decimal ActualGoalContributions,
    decimal ActualUnplannedGoalContributions,
    ReceivedIncomeBySource ReceivedIncome)
{
    /// <summary>
    /// Gets tracked income received during the Accounting Period that was planned as one of its Expected Income
    /// Sources.
    /// </summary>
    public decimal ActualPlannedTrackedIncome => ActualTrackedIncome - ActualUnplannedTrackedIncome;

}

/// <summary>
/// Represents an Accounting Period with the opening and closing balances stored for it, before its income and Fund Goal
/// totals are calculated.
/// </summary>
public sealed record StoredAccountingPeriodBalance(AccountingPeriod AccountingPeriod, decimal OpeningBalance, decimal ClosingBalance);

/// <summary>
/// Represents an Accounting Period with its Expected Income Sources and the posted income linked to each of them.
/// </summary>
public sealed record AccountingPeriodExpectedIncome(AccountingPeriod AccountingPeriod, ReceivedIncomeBySource ReceivedIncome);

/// <summary>
/// Posted income received from an Expected Income Source, in total and to tracked Accounts.
/// </summary>
public sealed record ReceivedIncome(decimal Total, decimal Tracked)
{
    /// <summary>
    /// Totals the posted income in the provided facts.
    /// </summary>
    public static ReceivedIncome From(IEnumerable<FinancialRangeIncomeFact> incomeFacts)
    {
        var posted = incomeFacts.Where(fact => fact.PostedDate != null).ToList();
        return new ReceivedIncome(
            posted.Sum(fact => fact.Amount),
            posted.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount));
    }
}

/// <summary>
/// The posted income received from each Expected Income Source.
/// </summary>
public sealed class ReceivedIncomeBySource
{
    private readonly IReadOnlyDictionary<ExpectedIncomeSourceId, ReceivedIncome> _receivedIncome;

    /// <summary>
    /// Received income for no Expected Income Sources.
    /// </summary>
    public static ReceivedIncomeBySource None { get; } = new(new Dictionary<ExpectedIncomeSourceId, ReceivedIncome>());

    /// <summary>
    /// Totals the posted income in the provided facts by the Expected Income Source it is linked to.
    /// </summary>
    public static ReceivedIncomeBySource From(IEnumerable<FinancialRangeIncomeFact> incomeFacts) => new(incomeFacts
        .Where(fact => fact.ExpectedIncomeSourceId != null)
        .GroupBy(fact => fact.ExpectedIncomeSourceId!)
        .ToDictionary(group => group.Key, ReceivedIncome.From));

    /// <summary>
    /// Gets the posted income linked to the provided Expected Income Source.
    /// </summary>
    public ReceivedIncome Get(ExpectedIncomeSourceId expectedIncomeSourceId) =>
        _receivedIncome.GetValueOrDefault(expectedIncomeSourceId) ?? new ReceivedIncome(0, 0);

    private ReceivedIncomeBySource(IReadOnlyDictionary<ExpectedIncomeSourceId, ReceivedIncome> receivedIncome)
    {
        _receivedIncome = receivedIncome;
    }
}
