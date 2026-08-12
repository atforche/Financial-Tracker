namespace Domain.Payroll;

/// <summary>
/// Indicates the wage bases affected by an earning or deduction.
/// </summary>
[Flags]
public enum PayrollTaxTreatment
{
    /// <summary>
    /// No supported wage base.
    /// </summary>
    None = 0,

    /// <summary>
    /// Federal income-tax wages.
    /// </summary>
    FederalIncome = 1,

    /// <summary>
    /// Social Security wages.
    /// </summary>
    SocialSecurity = 2,

    /// <summary>
    /// Medicare wages.
    /// </summary>
    Medicare = 4,

    /// <summary>
    /// State income-tax wages.
    /// </summary>
    StateIncome = 8,

    /// <summary>
    /// All supported wage bases.
    /// </summary>
    FullyTaxable = FederalIncome | SocialSecurity | Medicare | StateIncome,
}