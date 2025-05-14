using System.Text.Json.Serialization;
using Domain.Accounts;

namespace Rest.Models.AccountBalance;

/// <summary>
/// REST model representing an Account Balance for a particular date
/// </summary>
public class AccountBalanceByDateModel
{
    /// <summary>
    /// Date for this Account Balance By Date Model
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Account Balance for this Account Balance By Date Model
    /// </summary>
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