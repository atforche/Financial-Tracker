namespace Domain.BalanceEvents;

/// <summary>
/// Comparer class for <see cref="IBalanceEvent"/>
/// </summary>
public class BalanceEventComparer : IComparer<IBalanceEvent>
{
    /// <inheritdoc/>
    public int Compare(IBalanceEvent? first, IBalanceEvent? second)
    {
        if (first == null && second == null)
        {
            return 0;
        }
        if (first == null)
        {
            return -1;
        }
        if (second == null)
        {
            return 1;
        }
        if (first.EventDate != second.EventDate)
        {
            return first.EventDate.CompareTo(second.EventDate);
        }
        return first.EventSequence.CompareTo(second.EventSequence);
    }
}