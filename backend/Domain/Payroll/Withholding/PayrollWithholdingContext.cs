namespace Domain.Payroll.Withholding;

/// <summary>
/// Current and year-to-date facts needed to calculate one payroll withholding.
/// </summary>
public sealed record PayrollWithholdingContext(
    DateOnly PaymentDate,
    PayrollTaxableWages CurrentWages,
    PayrollTaxableWages YearToDateWagesBeforePayment,
    IReadOnlyCollection<PayrollTaxWithholding> YearToDateWithholdingsBeforePayment)
{
    internal PayrollWithholdingContext Snapshot() => new(
        PaymentDate,
        CurrentWages.Snapshot(),
        YearToDateWagesBeforePayment.Snapshot(),
        YearToDateWithholdingsBeforePayment.Select(withholding => withholding.Snapshot()).ToList().AsReadOnly());
}