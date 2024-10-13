using Domain.Factories;

namespace Domain.Entities;

/// <summary>
/// Class representing an Account Starting Balance entity
/// </summary>
public class AccountStartingBalance
{
    /// <summary>
    /// Id of this Account Starting Balance
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Id of the Account this starting balance if for
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Id of the Accounting Period this starting balance is for
    /// </summary>
    public Guid AccountingPeriodId { get; }

    /// <summary>
    /// Starting balance of this Account for this Accounting Period
    /// </summary>
    public decimal StartingBalance { get; }

    /// <summary>
    /// Verifies that the current Account Starting Balance is valid
    /// </summary>
    public void Validate()
    {
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountingPeriodId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of an Account Starting Balance
    /// </summary>
    public class AccountStartingBalanceFactory : IAccountStartingBalanceFactory
    {
        /// <inheritdoc/>
        public AccountStartingBalance Create(Account account, CreateAccountStartingBalanceRequest request)
        {
            var accountStartingBalance = new AccountStartingBalance(Guid.NewGuid(),
                account.Id,
                request.AccountingPeriod.Id,
                request.StartingBalance);
            accountStartingBalance.Validate();
            return accountStartingBalance;
        }

        /// <inheritdoc/>
        public AccountStartingBalance Recreate(IRecreateAccountStartingBalanceRequest request) =>
            new AccountStartingBalance(request.Id, request.AccountId, request.AccountingPeriodId, request.StartingBalance);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id of this Account Starting Balance</param>
    /// <param name="accountId">Account ID for thie Account Starting Balance</param>
    /// <param name="accountingPeriodId">Accounting Period ID for this Account Starting Balance</param>
    /// <param name="startingBalance">Starting Balance for this Account Starting Balance</param>
    private AccountStartingBalance(Guid id, Guid accountId, Guid accountingPeriodId, decimal startingBalance)
    {
        Id = id;
        AccountId = accountId;
        AccountingPeriodId = accountingPeriodId;
        StartingBalance = startingBalance;
    }
}