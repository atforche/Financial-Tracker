namespace Domain.Payroll.Withholding;

/// <summary>
/// Taxable wage bases for one payroll payment.
/// </summary>
public sealed record PayrollTaxableWages(
    decimal FederalIncome,
    decimal SocialSecurity,
    decimal Medicare,
    decimal StateIncome)
{
    internal PayrollTaxableWages Snapshot() => new(
        FederalIncome,
        SocialSecurity,
        Medicare,
        StateIncome);
}