using Domain.Accounts;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents interpreted income and spending totals for a financial range.
/// </summary>
public sealed record FinancialRangeTotals(decimal TotalIncome, decimal TrackedIncome, decimal TotalSpending)
{
    /// <summary>
    /// Calculates financial totals from persisted income and spending facts.
    /// </summary>
    public static FinancialRangeTotals Calculate(
        IEnumerable<FinancialRangeIncomeFact> incomeFacts,
        IEnumerable<FinancialRangeSpendingFact> spendingFacts)
    {
        var recognizedIncome = incomeFacts
            .Where(fact => fact.PostedDate != null)
            .ToList();
        return new FinancialRangeTotals(
            recognizedIncome.Sum(fact => fact.Amount),
            recognizedIncome.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount),
            spendingFacts.Where(fact => fact.PostedDate != null).Sum(fact => fact.Amount));
    }
}
