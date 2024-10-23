using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entity class representing an Account Starting Balance
/// </summary>
/// <remarks>
/// An Account Starting Balance represents the balance of an Account at the 
/// beginning of an Accounting Period. This saved balance serves as a checkpoint 
/// to improve the efficiency of calculating the balance of an Account at any point in time.
/// </remarks>
public class AccountStartingBalance
{
    /// <summary>
    /// ID for this Account Starting Balance
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// ID of the Account for this Account Starting Balance
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// ID of the Accounting Period for this Account Starting Balance
    /// </summary>
    public Guid AccountingPeriodId { get; }

    /// <summary>
    /// List of Starting Fund Balances for this Account Starting Balance
    /// </summary>
    public ICollection<FundAmount> StartingFundBalances { get; }

    /// <summary>
    /// Reconstructs an existing Account Starting Balance
    /// </summary>
    /// <param name="request">Request to recreate an Account Starting Balance</param>
    public AccountStartingBalance(IRecreateAccountStartingBalanceRequest request)
    {
        Id = request.Id;
        AccountId = request.AccountId;
        AccountingPeriodId = request.AccountingPeriodId;
        StartingFundBalances = request.StartingFundBalances
            .Select(request => new FundAmount(request.FundId, request.Amount)).ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Account for this Account Starting Balance</param>
    /// <param name="accountingPeriod">Accounting Period for this Account Starting Balance</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account Starting Balance</param>
    internal AccountStartingBalance(Account account,
        AccountingPeriod accountingPeriod,
        IEnumerable<FundAmount> startingFundBalances)
    {
        Id = Guid.NewGuid();
        AccountId = account.Id;
        AccountingPeriodId = accountingPeriod.Id;
        StartingFundBalances = startingFundBalances.ToList();
        Validate();
    }

    /// <summary>
    /// Validates the current Account Starting Balance
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountingPeriodId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (StartingFundBalances.Sum(balance => balance.Amount) < 0)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Interface representing a request to recreate an existing Account Starting Balance
/// </summary>
public interface IRecreateAccountStartingBalanceRequest
{
    /// <inheritdoc cref="AccountStartingBalance.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="AccountStartingBalance.AccountId"/>
    Guid AccountId { get; }

    /// <inheritdoc cref="AccountStartingBalance.AccountingPeriodId"/>
    Guid AccountingPeriodId { get; }

    /// <inheritdoc cref="AccountStartingBalance.StartingFundBalances"/>
    IEnumerable<IRecreateFundAmountRequest> StartingFundBalances { get; }
}