using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.ChangeInValues.Exceptions;
using Domain.Funds;

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
    protected override bool ValidatePrivate(CreateChangeInValueRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.FundAmount.Amount == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException());
        }
        return !exceptions.Any();
    }
}

/// <summary>
/// Record representing a request to create a <see cref="ChangeInValue"/>
/// </summary>
public record CreateChangeInValueRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Account ID for the Change in Value
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Fund Amount for the Change in Value
    /// </summary>
    public required FundAmount FundAmount { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyCollection<AccountId> GetAccountIds() => [AccountId];
}