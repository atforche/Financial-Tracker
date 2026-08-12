using Models.Income;

namespace Tests.Transactions;

internal static class IncomeBreakdownModelFactory
{
    public static IncomeBreakdownRequestModel Simple(decimal trackedAmount, decimal untrackedAmount = 0m) => new()
    {
        Kind = IncomeBreakdownKindModel.Simple,
        TrackedAmount = trackedAmount,
        UntrackedAmount = untrackedAmount,
        Earnings = [],
        EmployeeDeductions = [],
        EmployerContributions = [],
        TaxWithholdings = []
    };

    public static IncomeBreakdownRequestModel Payroll(
        IReadOnlyCollection<(string Description, decimal Amount)> earnings,
        IReadOnlyCollection<(string Description, decimal Amount)> employeeDeductions) =>
        new()
        {
            Kind = IncomeBreakdownKindModel.Payroll,
            Earnings = earnings.Select(earning => new PayrollEarningModel
            {
                Description = earning.Description,
                Amount = earning.Amount,
            }).ToArray(),
            EmployeeDeductions = employeeDeductions.Select(deduction => new EmployeePayrollDeductionModel
            {
                Description = deduction.Description,
                Amount = deduction.Amount,
                Disposition = 0,
                ReducesTaxableWagesFor = 0
            }).ToArray(),
            EmployerContributions = [],
            TaxWithholdings = []
        };
}