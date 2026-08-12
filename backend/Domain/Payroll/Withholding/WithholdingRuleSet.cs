namespace Domain.Payroll.Withholding;

/// <summary>
/// Published, versioned rules consumed by a jurisdiction-specific withholding calculator.
/// </summary>
public abstract record WithholdingRuleSet(WithholdingRuleSetReference Reference)
{
    /// <summary>
    /// Creates a detached snapshot of the published rules.
    /// </summary>
    public abstract WithholdingRuleSet Snapshot();
}