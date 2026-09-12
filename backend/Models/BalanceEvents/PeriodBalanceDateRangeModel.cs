namespace Models.BalanceEvents;

/// <summary>Daily balances and opening balance for an Accounting Period.</summary>
public sealed class PeriodBalanceDateRangeModel
{
    /// <summary>Balance before the first date in Dates. Dates may begin before the calendar month when a movement assigned to this Accounting Period posted earlier.</summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>One balance per day across the calendar month, extended to include any earlier or later posted movements assigned to this Accounting Period.</summary>
    public required IReadOnlyCollection<PeriodBalanceDateModel> Dates { get; init; }
}
