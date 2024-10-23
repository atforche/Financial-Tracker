namespace RestApi.Models.AccountBalance;

/// <summary>
/// REST model representing an Accounts balance for a particular date
/// </summary>
public class AccountBalanceByDateModel
{
    /// <summary>
    /// Date for this Account Balance By Date Model
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Account Balance for this Account Balance By Date Model
    /// </summary>
    public required AccountBalanceModel AccountBalance { get; init; }
}