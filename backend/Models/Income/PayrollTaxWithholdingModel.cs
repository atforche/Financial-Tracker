namespace Models.Income;

/// <summary>
/// Tax withheld from a payroll payment.
/// </summary>
public sealed class PayrollTaxWithholdingModel
{
    /// <summary>
    /// Jurisdiction receiving the tax.
    /// </summary>
    public required PayrollTaxJurisdictionModel Jurisdiction { get; init; }

    /// <summary>
    /// Payroll tax category.
    /// </summary>
    public required int TaxType { get; init; }

    /// <summary>
    /// Amount withheld.
    /// </summary>
    public required decimal Amount { get; init; }
}