using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
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
    protected override bool ValidatePrivate(CreateFundConversionRequest request, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (request.FromFundId == request.ToFundId)
        {
            exception = new InvalidOperationException();
        }
        if (request.Amount <= 0)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}

/// <summary>
/// Record representing a request to create a <see cref="FundConversion"/>
/// </summary>
public record CreateFundConversionRequest : CreateBalanceEventRequest
{
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
}