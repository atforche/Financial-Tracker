namespace Domain.Payroll.Withholding;

/// <summary>
/// Versioned published rules used by a withholding calculator.
/// </summary>
public sealed record WithholdingRuleSetReference(
    PayrollTaxJurisdiction Jurisdiction,
    int TaxYear,
    string Revision,
    DateOnly EffectiveDate);