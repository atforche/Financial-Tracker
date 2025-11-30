using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.FundConversions.Exceptions;
using Domain.Funds;

namespace Domain.FundConversions;

/// <summary>
/// Factory for building a <see cref="FundConversion"/>
/// </summary>
public class FundConversionFactory(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    : BalanceEventFactory<FundConversion, CreateFundConversionRequest>(accountingPeriodRepository,
        accountRepository,
        balanceEventRepository,
        accountBalanceService)
{
    /// <inheritdoc/>
    protected override FundConversion CreatePrivate(CreateFundConversionRequest request) =>
        new(request.AccountingPeriodId,
            request.EventDate,
            GetBalanceEventSequence(request.EventDate),
            request.AccountId,
            request.FromFundId,
            request.ToFundId,
            request.Amount);

    /// <inheritdoc/>
    protected override bool ValidatePrivate(CreateFundConversionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.FromFundId == request.ToFundId)
        {
            exceptions = exceptions.Append(new InvalidFundsException());
        }
        if (request.Amount <= 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException());
        }
        return !exceptions.Any();
    }
}

/// <summary>
/// Record representing a request to create a <see cref="FundConversion"/>
/// </summary>
public record CreateFundConversionRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Account ID for the Fund Conversion
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// From Fund ID for the Fund Conversion
    /// </summary>
    public required FundId FromFundId { get; init; }

    /// <summary>
    /// To Fund ID for the Fund Conversion
    /// </summary>
    public required FundId ToFundId { get; init; }

    /// <summary>
    /// Amount for the Fund Conversion
    /// </summary>
    public required decimal Amount { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyCollection<AccountId> GetAccountIds() => [AccountId];
}