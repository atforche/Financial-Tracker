using System.Text.Json.Serialization;
using Domain.Accounts;

namespace Rest.Models.Accounts;

/// <summary>
/// REST model representing an <see cref="AccountBalanceByDate"/>
/// </summary>
public class AccountBalanceByDateModel
{
    /// <inheritdoc cref="AccountBalanceByDate.Date"/>
    public DateOnly Date { get; init; }

    /// <inheritdoc cref="AccountBalanceByDate.AccountBalance"/>
    public AccountBalanceModel AccountBalance { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public AccountBalanceByDateModel(DateOnly date, AccountBalanceModel accountBalance)
    {
        Date = date;
        AccountBalance = accountBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByDate">
    /// Account Balance By Date record to build this Account Balance By Date REST model from
    /// </param>
    public AccountBalanceByDateModel(AccountBalanceByDate accountBalanceByDate)
    {
        Date = accountBalanceByDate.Date;
        AccountBalance = new AccountBalanceModel(accountBalanceByDate.AccountBalance);
    }
}