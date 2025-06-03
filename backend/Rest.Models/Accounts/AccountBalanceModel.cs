using System.Text.Json.Serialization;
using Domain.Accounts;
using Rest.Models.Funds;

namespace Rest.Models.Accounts;

/// <summary>
/// REST model representing an <see cref="AccountBalance"/>
/// </summary>
public class AccountBalanceModel
{
    /// <inheritdoc cref="AccountBalance.FundBalances"/>
    public IReadOnlyCollection<FundAmountModel> FundBalances { get; init; }

    /// <inheritdoc cref="AccountBalance.PendingFundBalanceChanges"/>
    public IReadOnlyCollection<FundAmountModel> PendingFundBalanceChanges { get; init; }

    /// <inheritdoc cref="AccountBalance.Balance"/>
    public decimal Balance { get; init; }

    /// <inheritdoc cref="AccountBalance.TotalPendingBalanceChange"/>
    public decimal TotalPendingBalanceChange { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public AccountBalanceModel(IEnumerable<FundAmountModel> fundBalances,
        IEnumerable<FundAmountModel> pendingFundBalanceChanges,
        decimal balance,
        decimal totalPendingBalanceChange)
    {
        FundBalances = fundBalances.ToList();
        PendingFundBalanceChanges = pendingFundBalanceChanges.ToList();
        Balance = balance;
        TotalPendingBalanceChange = totalPendingBalanceChange;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalance">Account Balance value object to build this Account Balance REST model from</param>
    public AccountBalanceModel(AccountBalance accountBalance)
    {
        FundBalances = accountBalance.FundBalances.Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
        PendingFundBalanceChanges = accountBalance.PendingFundBalanceChanges
            .Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
        Balance = accountBalance.Balance;
        TotalPendingBalanceChange = accountBalance.TotalPendingBalanceChange;
    }
}