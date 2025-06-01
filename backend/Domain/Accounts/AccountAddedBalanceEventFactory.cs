using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Services;

namespace Domain.Accounts;

/// <summary>
/// Factory for building a <see cref="Account"/>
/// </summary>
public class AccountAddedBalanceEventFactory(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    : BalanceEventFactory<AccountAddedBalanceEvent, CreateAccountAddedBalanceEventRequest>(
        accountingPeriodRepository,
        accountRepository,
        balanceEventRepository,
        accountBalanceService)
{
    /// <inheritdoc/>
    protected override AccountAddedBalanceEvent CreatePrivate(CreateAccountAddedBalanceEventRequest request) =>
        new(request.Account,
            request.AccountingPeriodId,
            request.EventDate,
            GetBalanceEventSequence(request.EventDate),
            request.FundAmounts);

    /// <inheritdoc/>
    protected override bool ValidatePrivate(CreateAccountAddedBalanceEventRequest request, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (request.FundAmounts.GroupBy(amount => amount.FundId).Any(group => group.Count() > 1))
        {
            exception = new InvalidOperationException();
        }
        if (request.FundAmounts.Any(amount => amount.Amount <= 0))
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}

/// <summary>
/// Record representing a request to create a <see cref="AccountAddedBalanceEvent"/>
/// </summary>
public record CreateAccountAddedBalanceEventRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Account for the Account Added Balance Event
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Fund Amounts for the Account Added Balance Event
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAmounts { get; init; }
}