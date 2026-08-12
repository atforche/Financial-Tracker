using Domain.Income;
using Domain.Payroll.Withholding;

namespace Domain.Payroll;

/// <summary>
/// A payroll payment, shared by expected income and actual income transactions.
/// </summary>
public class PayrollPayment : IncomeBreakdown
{
    private readonly List<PayrollEarning> _earnings = [];
    private readonly List<EmployeePayrollDeduction> _employeeDeductions = [];
    private readonly List<EmployerContribution> _employerContributions = [];
    private readonly List<PayrollTaxWithholding> _taxWithholdings = [];

    /// <summary>
    /// State in which this payroll payment's state income wage base applies.
    /// </summary>
    public string? StateIncomeStateCode { get; private set; }

    /// <summary>
    /// Cash compensation paid by the employer before deductions and withholding.
    /// </summary>
    public IReadOnlyCollection<PayrollEarning> Earnings => _earnings.AsReadOnly();

    /// <summary>
    /// Employee deductions taken from cash compensation.
    /// </summary>
    public IReadOnlyCollection<EmployeePayrollDeduction> EmployeeDeductions => _employeeDeductions.AsReadOnly();

    /// <summary>
    /// Employer-funded compensation which does not enter the employee's cash deposit.
    /// </summary>
    public IReadOnlyCollection<EmployerContribution> EmployerContributions => _employerContributions.AsReadOnly();

    /// <summary>
    /// Taxes withheld from cash compensation.
    /// </summary>
    public IReadOnlyCollection<PayrollTaxWithholding> TaxWithholdings => _taxWithholdings.AsReadOnly();

    /// <summary>
    /// Gross cash compensation before deductions and withholding.
    /// </summary>
    public decimal GrossCashAmount => Earnings.Sum(earning => earning.Amount);

    /// <summary>
    /// Employee deductions which become income in an untracked destination.
    /// </summary>
    public decimal EmployeeUntrackedContributionAmount => EmployeeDeductions
        .Where(deduction => deduction.Disposition == EmployeeDeductionDisposition.UntrackedContribution)
        .Sum(deduction => deduction.Amount);

    /// <summary>
    /// Deductions which reduce recognized income rather than moving it to an untracked destination.
    /// </summary>
    public decimal NonIncomeDeductionAmount => EmployeeDeductions
        .Where(deduction => deduction.Disposition == EmployeeDeductionDisposition.NonIncomeDeduction)
        .Sum(deduction => deduction.Amount);

    /// <summary>
    /// Total taxes withheld from the payment.
    /// </summary>
    public decimal TaxWithholdingAmount => TaxWithholdings.Sum(withholding => withholding.Amount);

    /// <summary>
    /// Calculates the wage bases to which the supported payroll taxes apply.
    /// </summary>
    public PayrollTaxableWages GetTaxableWages() => new(
        CalculateTaxableWages(PayrollTaxTreatment.FederalIncome),
        CalculateTaxableWages(PayrollTaxTreatment.SocialSecurity),
        CalculateTaxableWages(PayrollTaxTreatment.Medicare),
        CalculateStateIncomeWages());

    /// <inheritdoc/>
    public override decimal TrackedAmount => GrossCashAmount
        - EmployeeDeductions.Sum(deduction => deduction.Amount)
        - TaxWithholdingAmount;

    /// <inheritdoc/>
    public override decimal UntrackedAmount => EmployeeUntrackedContributionAmount
        + EmployerContributions.Sum(contribution => contribution.Amount);

    /// <summary>
    /// Constructs a payroll payment.
    /// </summary>
    public PayrollPayment(
        IEnumerable<PayrollEarning> earnings,
        IEnumerable<EmployeePayrollDeduction> employeeDeductions,
        IEnumerable<EmployerContribution> employerContributions,
        IEnumerable<PayrollTaxWithholding> taxWithholdings,
        string? stateIncomeStateCode = null)
        : base(new IncomeBreakdownId(Guid.NewGuid()))
    {
        _earnings.AddRange(earnings);
        _employeeDeductions.AddRange(employeeDeductions);
        _employerContributions.AddRange(employerContributions);
        _taxWithholdings.AddRange(taxWithholdings);
        StateIncomeStateCode = stateIncomeStateCode;
    }

    /// <summary>
    /// Constructs a default instance for persistence.
    /// </summary>
    protected PayrollPayment() : base() { }

    /// <summary>
    /// Creates a detached copy of this payroll payment.
    /// </summary>
    public virtual PayrollPayment Snapshot() => new(
        Earnings.Select(earning => earning.Snapshot()),
        EmployeeDeductions.Select(deduction => deduction.Snapshot()),
        EmployerContributions.Select(contribution => contribution.Snapshot()),
        TaxWithholdings.Select(withholding => withholding.Snapshot()),
        StateIncomeStateCode);

    /// <summary>
    /// Replaces the payment details while preserving its persistence identity.
    /// </summary>
    internal void UpdateFrom(PayrollPayment payment)
    {
        _earnings.Clear();
        _earnings.AddRange(payment.Earnings.Select(earning => earning.Snapshot()));
        _employeeDeductions.Clear();
        _employeeDeductions.AddRange(payment.EmployeeDeductions.Select(deduction => deduction.Snapshot()));
        _employerContributions.Clear();
        _employerContributions.AddRange(payment.EmployerContributions.Select(contribution => contribution.Snapshot()));
        _taxWithholdings.Clear();
        _taxWithholdings.AddRange(payment.TaxWithholdings.Select(withholding => withholding.Snapshot()));
        StateIncomeStateCode = payment.StateIncomeStateCode;
    }

    private decimal CalculateStateIncomeWages() =>
        string.IsNullOrWhiteSpace(StateIncomeStateCode)
            ? 0
            : CalculateTaxableWages(PayrollTaxTreatment.StateIncome);

    private decimal CalculateTaxableWages(PayrollTaxTreatment treatment) =>
        Earnings.Sum(earning => earning.Amount)
        - EmployeeDeductions.Where(deduction => deduction.ReducesTaxableWagesFor.HasFlag(treatment)).Sum(deduction => deduction.Amount);
}