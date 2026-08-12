namespace Domain.Payroll.Withholding;

/// <summary>
/// A tax withheld from a payroll payment.
/// </summary>
public sealed class PayrollTaxWithholding(
    PayrollTaxJurisdiction jurisdiction,
    PayrollTaxType taxType,
    decimal amount)
{
    /// <summary>
    /// Jurisdiction receiving the withheld tax.
    /// </summary>
    public PayrollTaxJurisdiction Jurisdiction { get; private set; } = jurisdiction;

    /// <summary>
    /// Kind of tax withheld.
    /// </summary>
    public PayrollTaxType TaxType { get; private set; } = taxType;

    /// <summary>
    /// Amount withheld.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    internal PayrollTaxWithholding Snapshot() => new(Jurisdiction with { }, TaxType, Amount);

    private PayrollTaxWithholding() : this(null!, default, 0) { }
}