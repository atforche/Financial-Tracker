namespace Domain.Payroll.Withholding;

/// <summary>
/// Identifies the government jurisdiction imposing a payroll tax.
/// </summary>
public sealed record PayrollTaxJurisdiction(string CountryCode, string? SubdivisionCode = null, string? Locality = null)
{
    /// <summary>
    /// United States federal jurisdiction.
    /// </summary>
    public static PayrollTaxJurisdiction UnitedStatesFederal { get; } = new("US");
}