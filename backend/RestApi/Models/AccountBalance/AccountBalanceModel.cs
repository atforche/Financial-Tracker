using RestApi.Models.FundAmount;

namespace RestApi.Models.AccountBalance;

/// <summary>
/// REST model representing an Account's balance at a particular moment in time
/// </summary>
public class AccountBalanceModel
{
    /// <inheritdoc cref="Domain.ValueObjects.AccountBalance.FundBalances"/>
    public required ICollection<FundAmountModel> FundBalances { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.AccountBalance.FundBalancesIncludingPendingTransactions"/>
    public required ICollection<FundAmountModel> FundBalancesIncludingPendingTransactions { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.AccountBalance.Balance"/>
    public required decimal Balance { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.AccountBalance.BalanceIncludingPendingTransactions"/>
    public required decimal BalanceIncludingPendingTransactions { get; init; }

    /// <summary>
    /// Converts the Account Balance domain value object into an Account Balance REST model
    /// </summary>
    /// <param name="accountBalance">Account Balance domain value object to be converted</param>
    /// <returns>The converted Account Balance REST model</returns>
    internal static AccountBalanceModel ConvertValueObjectToModel(Domain.ValueObjects.AccountBalance accountBalance) =>
        new AccountBalanceModel
        {
            FundBalances = accountBalance.FundBalances.Select(FundAmountModel.ConvertValueObjectToModel).ToList(),
            FundBalancesIncludingPendingTransactions = accountBalance.FundBalancesIncludingPendingTransactions
                .Select(FundAmountModel.ConvertValueObjectToModel).ToList(),
            Balance = accountBalance.Balance,
            BalanceIncludingPendingTransactions = accountBalance.BalanceIncludingPendingTransactions,
        };
}