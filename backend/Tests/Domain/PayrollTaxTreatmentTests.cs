using Domain.Payroll;

namespace Tests.Domain;

/// <summary>
/// Covers payroll wage-base treatment flags.
/// </summary>
public sealed class PayrollTaxTreatmentTests
{
    /// <summary>
    /// Calculates the state wage base from all earnings and state-applicable deductions.
    /// </summary>
    [Fact]
    public void StateIncomeTreatmentControlsStateWages()
    {
        var payment = new PayrollPayment(
            [
                new PayrollEarning("State wages", 100m),
                new PayrollEarning("Additional wages", 50m),
            ],
            [
                new EmployeePayrollDeduction(
                    "State deduction",
                    20m,
                    EmployeeDeductionDisposition.NonIncomeDeduction,
                    PayrollTaxTreatment.StateIncome),
                new EmployeePayrollDeduction(
                    "Non-state deduction",
                    10m,
                    EmployeeDeductionDisposition.NonIncomeDeduction,
                    PayrollTaxTreatment.None),
            ],
            [],
            [],
            "NY");

        Assert.Equal(130m, payment.GetTaxableWages().StateIncome);
    }

    /// <summary>
    /// Keeps the all-taxable convenience value inclusive of state income wages.
    /// </summary>
    [Fact]
    public void FullyTaxableIncludesStateIncome() =>
        Assert.True(PayrollTaxTreatment.FullyTaxable.HasFlag(PayrollTaxTreatment.StateIncome));
}