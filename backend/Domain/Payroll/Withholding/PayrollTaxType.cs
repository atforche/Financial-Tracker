namespace Domain.Payroll.Withholding;

/// <summary>
/// Supported payroll tax categories.
/// </summary>
public enum PayrollTaxType
{
    /// <summary>
    /// Income tax.
    /// </summary>
    Income,

    /// <summary>
    /// Social Security tax.
    /// </summary>
    SocialSecurity,

    /// <summary>
    /// Medicare tax.
    /// </summary>
    Medicare,

    /// <summary>
    /// Local payroll or income tax.
    /// </summary>
    Local,
}