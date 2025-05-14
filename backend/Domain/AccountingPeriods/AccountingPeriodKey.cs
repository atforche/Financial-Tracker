namespace Domain.AccountingPeriods;

/// <summary>
/// Value object class representing the Year and Month of an Accounting Period
/// </summary>
/// <param name="Year">Year of the Accounting Period</param>
/// <param name="Month">Month of the Accounting Period</param>
public record AccountingPeriodKey(int Year, int Month) : IComparable<AccountingPeriodKey>
{
    /// <summary>
    /// Gets the Date representation of this Accounting Period Key
    /// </summary>
    /// <returns>The Date representation of this Accounting Period Key</returns>
    public DateOnly ConvertToDate() => new(Year, Month, 1);

    #region IComparable

    /// <inheritdoc/>
    public int CompareTo(AccountingPeriodKey? other)
    {
        if (other is null)
        {
            return 1;
        }
        if (Year != other.Year)
        {
            return Year.CompareTo(other.Year);
        }
        return Month.CompareTo(other.Month);
    }

    /// <inheritdoc/>
    public static bool operator >(AccountingPeriodKey operand1, AccountingPeriodKey operand2) => operand1.CompareTo(operand2) > 0;

    /// <inheritdoc/>
    public static bool operator <(AccountingPeriodKey operand1, AccountingPeriodKey operand2) => operand1.CompareTo(operand2) < 0;

    /// <inheritdoc/>
    public static bool operator >=(AccountingPeriodKey operand1, AccountingPeriodKey operand2) => operand1.CompareTo(operand2) >= 0;

    /// <inheritdoc/>
    public static bool operator <=(AccountingPeriodKey operand1, AccountingPeriodKey operand2) => operand1.CompareTo(operand2) <= 0;

    #endregion
}