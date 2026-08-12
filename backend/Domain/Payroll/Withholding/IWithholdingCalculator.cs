namespace Domain.Payroll.Withholding;

/// <summary>
/// Calculates withholding without coupling payroll to a particular jurisdiction's algorithm.
/// </summary>
public interface IWithholdingCalculator
{
    /// <summary>Determines whether this calculator supports the supplied election and rules.</summary>
    bool Supports(WithholdingElection election, WithholdingRuleSet rules);

    /// <summary>Calculates withholding for one payroll payment.</summary>
    IReadOnlyCollection<PayrollTaxWithholding> Calculate(
        PayrollWithholdingContext context,
        WithholdingElection election,
        WithholdingRuleSet rules,
        int payPeriodsPerYear);
}