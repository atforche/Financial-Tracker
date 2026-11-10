using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.BalanceEvents;
using Models.Accounts;
using Models.BalanceEvents;
using Rest.AccountingPeriods;

namespace Rest.Accounts;

/// <summary>
/// Converts Account balance-event API models and Domain query results.
/// </summary>
public sealed class AccountBalanceEventConverter
{
    /// <summary>
    /// Converts the provided API Account balance-event query to a Domain query.
    /// </summary>
    public AccountBalanceEventAccountQuery ToDomain(
        Guid accountId,
        AccountBalanceEventsQueryParameterModel model) => new(
            accountId,
            model.AccountingPeriodIds == null ? model.Range?.Start : null,
            model.AccountingPeriodIds == null ? model.Range?.End : null,
            model.AccountingPeriodIds,
            model.IncludePending,
            model.Offset ?? 0,
            model.Limit);

    /// <summary>
    /// Converts the provided Domain Account balance event to an API model.
    /// </summary>
    public AccountBalanceEventModel ToModel(AccountBalanceEvent balanceEvent) => new()
    {
        AccountingPeriod = AccountingPeriodConverter.ToModel(balanceEvent.AccountingPeriod),
        TransactionId = balanceEvent.TransactionId.Value,
        Description = balanceEvent.Description,
        TransactionDate = balanceEvent.TransactionDate,
        TransactionSequence = balanceEvent.TransactionSequence,
        EventDate = balanceEvent.EventDate,
        EventDateSequence = balanceEvent.EventDateSequence,
        Type = balanceEvent.Type == BalanceEventType.Debit ? BalanceEventTypeModel.Debit : BalanceEventTypeModel.Credit,
        IsPosted = balanceEvent.IsPosted,
        Amount = balanceEvent.Amount,
        Account = new AccountModel
        {
            Id = balanceEvent.Account.Id.Value,
            Name = balanceEvent.Account.Name,
            FinancialInstitution = balanceEvent.Account.FinancialInstitution,
            Type = AccountTypeConverter.ToModel(balanceEvent.Account.Type),
        },
        Source = ToModel(balanceEvent.Source),
        Destinations = balanceEvent.Destinations.Select(ToModel).ToList(),
        PreviousBalance = balanceEvent.PreviousBalance == null ? null : ToModel(balanceEvent.PreviousBalance),
        NewBalance = balanceEvent.NewBalance == null ? null : ToModel(balanceEvent.NewBalance),
    };

    /// <summary>
    /// Converts a Domain Account balance event party to an API model.
    /// </summary>
    private static AccountBalanceEventPartyModel ToModel(AccountBalanceEventParty party) => new()
    {
        DisplayName = party.DisplayName,
        Amount = party.Amount,
    };

    /// <summary>
    /// Converts the provided Domain Account balance to an API model.
    /// </summary>
    private static AccountBalanceModel ToModel(AccountBalance balance) => new()
    {
        PostedBalance = balance.PostedBalance,
    };
}
