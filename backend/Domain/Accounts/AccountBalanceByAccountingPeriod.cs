using Domain.AccountingPeriods;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the starting and ending balances across an Accounting Period
/// </summary>
public record AccountBalanceByAccountingPeriod
{
    /// <summary>
    /// Accounting Period ID for this Account Balance by Accounting Period
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; }

    /// <summary>
    /// Starting Balance for this Account Balance by Accounting Period
    /// </summary>
    public AccountBalance StartingBalance { get; }

    /// <summary>
    /// Ending Balance for this Account Balance by Accounting Period
    /// </summary>
    public AccountBalance EndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Account Balance by Accounting Period</param>
    /// <param name="startingBalance">Starting Balance for this Account Balance by Accounting Period</param>
    /// <param name="endingBalance">Ending Balance for this Account Balance by Accounting Period</param>
    public AccountBalanceByAccountingPeriod(AccountingPeriod accountingPeriod,
        AccountBalance startingBalance,
        AccountBalance endingBalance)
    {
        AccountingPeriodId = accountingPeriod.Id;
        StartingBalance = startingBalance;
        EndingBalance = endingBalance;
        Validate(accountingPeriod);
    }

    /// <summary>
    /// Validates this Account Balance by Accounting Period
    /// </summary>
    private void Validate(AccountingPeriod accountingPeriod)
    {
        if (StartingBalance.PendingFundBalanceChanges.Count != 0)
        {
            throw new InvalidOperationException();
        }
        if (!accountingPeriod.IsOpen && EndingBalance.PendingFundBalanceChanges.Count != 0)
        {
            throw new InvalidOperationException();
        }
    }
}