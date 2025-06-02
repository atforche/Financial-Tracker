using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Services;

namespace Domain.ChangeInValues;

/// <summary>
/// Factory for building a <see cref="ChangeInValue"/>
/// </summary>
public class ChangeInValueFactory(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    : BalanceEventFactory<ChangeInValue, CreateChangeInValueRequest>(accountingPeriodRepository,
        accountRepository,
        balanceEventRepository,
        accountBalanceService)
{
    /// <inheritdoc/>
    protected override ChangeInValue CreatePrivate(CreateChangeInValueRequest request) =>
        new(request.AccountingPeriodId,
            request.EventDate,
            GetBalanceEventSequence(request.EventDate),
            request.AccountId,
            request.FundAmount);

    /// <inheritdoc/>
    protected override bool ValidatePrivate(CreateChangeInValueRequest request, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (request.FundAmount.Amount == 0)
        {
            exception = new InvalidOperationException();
        }
        return exception == null;
    }
}

/// <summary>
/// Record representing a request to create a <see cref="ChangeInValue"/>
/// </summary>
public record CreateChangeInValueRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Fund Amount for the Change in Value
    /// </summary>
    public required FundAmount FundAmount { get; init; }
}