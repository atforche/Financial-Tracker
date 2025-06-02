using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundConversions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.FundConversions;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Fund Conversions
/// </summary>
[ApiController]
[Route("/fundConversions")]
public sealed class FundConversionController(
    UnitOfWork unitOfWork,
    IFundConversionRepository fundConversionRepository,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    AccountIdFactory accountIdFactory,
    FundConversionFactory fundConversionFactory,
    FundIdFactory fundIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Fund Conversions for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Fund Conversions that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetFundConversionsByAccountingPeriod(Guid accountingPeriodId)
    {
        IEnumerable<FundConversion> fundConversions = fundConversionRepository.FindAllByAccountingPeriod(accountingPeriodIdFactory.Create(accountingPeriodId));
        return Ok(fundConversions.Select(fundConversion => new FundConversionModel(fundConversion)));
    }

    /// <summary>
    /// Creates a new Fund Conversion with the provided properties
    /// </summary>
    /// <param name="createFundConversionModel">Request to create a Fund Conversion</param>
    /// <returns>The created Fund Conversion</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateFundConversionAsync(CreateFundConversionModel createFundConversionModel)
    {
        FundConversion newFundConversion = fundConversionFactory.Create(new CreateFundConversionRequest
        {
            AccountingPeriodId = accountingPeriodIdFactory.Create(createFundConversionModel.AccountingPeriodId),
            EventDate = createFundConversionModel.EventDate,
            AccountId = accountIdFactory.Create(createFundConversionModel.AccountId),
            FromFundId = fundIdFactory.Create(createFundConversionModel.FromFundId),
            ToFundId = fundIdFactory.Create(createFundConversionModel.ToFundId),
            Amount = createFundConversionModel.Amount
        });
        fundConversionRepository.Add(newFundConversion);
        await unitOfWork.SaveChangesAsync();
        return Ok(new FundConversionModel(newFundConversion));
    }
}