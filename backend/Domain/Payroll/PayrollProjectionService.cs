using Domain.AccountingPeriods;
using Domain.Payroll.Withholding;
using Domain.Validation;

namespace Domain.Payroll;

/// <summary>
/// Projects expected payroll withholding from elections and published rules.
/// </summary>
public sealed class PayrollProjectionService(IEnumerable<IWithholdingCalculator> calculators)
{
    private readonly IReadOnlyCollection<IWithholdingCalculator> _calculators = calculators.ToList().AsReadOnly();

    /// <summary>
    /// Attempts to create an expected payroll payment with calculated withholding.
    /// </summary>
    public bool TryProject(
        IEnumerable<PayrollEarning> earnings,
        IEnumerable<EmployeePayrollDeduction> employeeDeductions,
        IEnumerable<EmployerContribution> employerContributions,
        int payPeriodsPerYear,
        PayrollWithholdingConfiguration configuration,
        PayrollWithholdingContext context,
        out ExpectedPayrollPayment? payment,
        out IEnumerable<ValidationError> errors)
    {
        payment = null;
        errors = [];
        PayrollPayment paymentWithoutWithholding = new(earnings, employeeDeductions, employerContributions, []);
        errors = errors.Concat(PayrollPaymentValidator.Validate(paymentWithoutWithholding, new ValidationErrorPath(nameof(ExpectedPayrollPayment))));
        if (payPeriodsPerYear is <= 0 or > 366)
        {
            errors = errors.Append(new ValidationError(new ValidationErrorPath(nameof(ExpectedPayrollPayment.PayPeriodsPerYear)), "Expected payroll income must specify between 1 and 366 pay periods per year."));
        }
        errors = errors.Concat(PayrollWithholdingConfigurationValidator.Validate(
            configuration,
            context.PaymentDate,
            new ValidationErrorPath(nameof(ExpectedPayrollPayment.WithholdingConfiguration))));
        PayrollTaxableWages currentWages = paymentWithoutWithholding.GetTaxableWages();
        PayrollWithholdingContext calculationContext = context with { CurrentWages = currentWages };
        IEnumerable<(WithholdingElection Election, WithholdingRuleSet Rules)> selections =
            new[] { (Election: (WithholdingElection)configuration.FederalElection, Rules: configuration.FederalRuleSet) }
                .Concat(configuration.StateSelections.Select(selection => (Election: (WithholdingElection)selection.Election, Rules: selection.RuleSet)));
        foreach ((WithholdingElection election, WithholdingRuleSet rules) in selections)
        {
            int calculatorCount = _calculators.Count(calculator => calculator.Supports(election, rules));
            if (calculatorCount != 1)
            {
                errors = errors.Append(new ValidationError(
                    new ValidationErrorPath(nameof(ExpectedPayrollPayment.WithholdingConfiguration)),
                    calculatorCount == 0
                        ? $"No withholding calculator supports {rules.Reference.Jurisdiction}."
                        : $"Multiple withholding calculators support {rules.Reference.Jurisdiction}."));
            }
        }
        if (errors.Any())
        {
            return false;
        }
        List<PayrollTaxWithholding> calculatedWithholdings = [];
        foreach ((WithholdingElection election, WithholdingRuleSet rules) in selections)
        {
            IReadOnlyCollection<PayrollTaxWithholding> selectionWithholdings = ResolveCalculator((election, rules)).Calculate(
                calculationContext,
                election,
                rules,
                payPeriodsPerYear);
            foreach (PayrollTaxWithholding withholding in selectionWithholdings)
            {
                if (!IsWithinJurisdiction(withholding.Jurisdiction, rules.Reference.Jurisdiction))
                {
                    errors = errors.Append(new ValidationError(
                        new ValidationErrorPath(nameof(ExpectedPayrollPayment.WithholdingConfiguration)),
                        "Calculated withholding jurisdiction must match its selected rule jurisdiction."));
                }
            }
            calculatedWithholdings.AddRange(selectionWithholdings);
        }
        if (errors.Any())
        {
            return false;
        }
        IReadOnlyCollection<PayrollTaxWithholding> withholdings = calculatedWithholdings.AsReadOnly();
        PayrollWithholdingConfiguration configurationSnapshot = configuration.Snapshot();
        PayrollWithholdingContext contextSnapshot = calculationContext.Snapshot();
        payment = new ExpectedPayrollPayment(
            paymentWithoutWithholding.Earnings,
            paymentWithoutWithholding.EmployeeDeductions,
            paymentWithoutWithholding.EmployerContributions,
            withholdings,
            payPeriodsPerYear,
            configurationSnapshot,
            contextSnapshot);
        errors = PayrollPaymentValidator.Validate(payment, new ValidationErrorPath(nameof(ExpectedPayrollPayment)));
        if (errors.Any())
        {
            payment = null;
            return false;
        }
        return true;
    }

    private IWithholdingCalculator ResolveCalculator((WithholdingElection Election, WithholdingRuleSet Rules) selection) =>
        _calculators.Single(calculator => calculator.Supports(selection.Election, selection.Rules));

    private static bool IsWithinJurisdiction(
        PayrollTaxJurisdiction calculated,
        PayrollTaxJurisdiction selected) => calculated == selected
            || (calculated.CountryCode == selected.CountryCode
                && calculated.SubdivisionCode == selected.SubdivisionCode
                && selected.Locality == null);
}