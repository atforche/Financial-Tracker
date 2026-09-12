namespace Models.BalanceEvents;

/// <summary>Posted balance at the end of a calendar day, scoped to an Accounting Period.</summary>
public sealed class PeriodBalanceDateModel
{
    /// <summary>Calendar date represented by this balance.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Opening balance plus posted movements assigned to the Accounting Period through this date.</summary>
    public required decimal TotalBalance { get; init; }
}
