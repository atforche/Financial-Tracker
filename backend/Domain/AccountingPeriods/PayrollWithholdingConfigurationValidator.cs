using Domain.Payroll.Withholding;
using Domain.Validation;

namespace Domain.AccountingPeriods;

/// <summary>
/// Validates withholding elections and their published rule sets.
/// </summary>
public static class PayrollWithholdingConfigurationValidator
{
    /// <summary>
    /// Validates a withholding configuration for an expected payment date.
    /// </summary>
    public static IEnumerable<ValidationError> Validate(
        PayrollWithholdingConfiguration configuration,
        DateOnly paymentDate,
        ValidationErrorPath path)
    {
        IEnumerable<ValidationError> errors = [];
        FederalWithholdingElection federal = configuration.FederalElection;
        if (string.IsNullOrWhiteSpace(federal.FormRevision))
        {
            errors = errors.Append(new ValidationError(path.Append(nameof(PayrollWithholdingConfiguration.FederalElection)), "Federal form revision is required."));
        }
        if (federal.DependentCreditAnnual < 0 || federal.OtherIncomeAnnual < 0
            || federal.DeductionsAnnual < 0 || federal.AdditionalWithholdingPerPayPeriod < 0)
        {
            errors = errors.Append(new ValidationError(path.Append(nameof(PayrollWithholdingConfiguration.FederalElection)), "Federal withholding amounts cannot be negative."));
        }
        errors = errors.Concat(ValidateRuleSet(
            configuration.FederalRuleSet,
            PayrollTaxJurisdiction.UnitedStatesFederal,
            paymentDate,
            path.Append(nameof(PayrollWithholdingConfiguration.FederalRuleSet))));
        foreach ((int index, StateWithholdingSelection selection) in configuration.StateSelections.Index())
        {
            ValidationErrorPath selectionPath = path.AppendWithIndex(nameof(PayrollWithholdingConfiguration.StateSelections), index);
            if (string.IsNullOrWhiteSpace(selection.Election.FormRevision))
            {
                errors = errors.Append(new ValidationError(selectionPath.Append(nameof(StateWithholdingSelection.Election)), "State form revision is required."));
            }
            if (selection.Election.AdditionalWithholdingPerPayPeriod < 0)
            {
                errors = errors.Append(new ValidationError(selectionPath.Append(nameof(StateWithholdingSelection.Election)), "Additional state withholding cannot be negative."));
            }
            errors = errors.Concat(ValidateRuleSet(selection.RuleSet, selection.Election.Jurisdiction, paymentDate, selectionPath.Append(nameof(StateWithholdingSelection.RuleSet))));
        }
        foreach (IGrouping<PayrollTaxJurisdiction, StateWithholdingSelection> duplicate in configuration.StateSelections.GroupBy(selection => selection.Election.Jurisdiction).Where(group => group.Count() > 1))
        {
            errors = errors.Append(new ValidationError(path.Append(nameof(PayrollWithholdingConfiguration.StateSelections)), $"Duplicate withholding selection for {duplicate.Key}."));
        }
        return errors;
    }

    private static IEnumerable<ValidationError> ValidateRuleSet(
        WithholdingRuleSet rules,
        PayrollTaxJurisdiction jurisdiction,
        DateOnly paymentDate,
        ValidationErrorPath path)
    {
        WithholdingRuleSetReference reference = rules.Reference;
        if (string.IsNullOrWhiteSpace(reference.Jurisdiction.CountryCode))
        {
            yield return new ValidationError(path, "Withholding rule country is required.");
        }
        if (reference.Jurisdiction != jurisdiction)
        {
            yield return new ValidationError(path, "Withholding rule jurisdiction must match the election jurisdiction.");
        }
        if (string.IsNullOrWhiteSpace(reference.Revision))
        {
            yield return new ValidationError(path, "Withholding rule revision is required.");
        }
        if (reference.TaxYear != paymentDate.Year || reference.EffectiveDate > paymentDate)
        {
            yield return new ValidationError(path, "Withholding rules must apply to the expected payment date.");
        }
    }
}