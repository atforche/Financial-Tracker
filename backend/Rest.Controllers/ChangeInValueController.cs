using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.ChangeInValues;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Change In Values
/// </summary>
[ApiController]
[Route("/changeInValues")]
public sealed class ChangeInValueController(
    UnitOfWork unitOfWork,
    IChangeInValueRepository changeInValueRepository,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    AccountIdFactory accountIdFactory,
    ChangeInValueFactory changeInValueFactory,
    FundIdFactory fundIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Change In Values for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Change In Values that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetByAccountingPeriod(Guid accountingPeriodId)
    {
        IEnumerable<ChangeInValue> changeInValues = changeInValueRepository.FindAllByAccountingPeriod(accountingPeriodIdFactory.Create(accountingPeriodId));
        return Ok(changeInValues.Select(changeInValue => new ChangeInValueModel(changeInValue)));
    }

    /// <summary>
    /// Creates a new Change In Value with the provided properties
    /// </summary>
    /// <param name="createChangeInValueModel">Request to create a Change In Value</param>
    /// <returns>The created Change In Value</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateChangeInValueModel createChangeInValueModel)
    {
        ChangeInValue newChangeInValue = changeInValueFactory.Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = accountingPeriodIdFactory.Create(createChangeInValueModel.AccountingPeriodId),
            EventDate = createChangeInValueModel.EventDate,
            AccountId = accountIdFactory.Create(createChangeInValueModel.AccountId),
            FundAmount = new FundAmount
            {
                FundId = fundIdFactory.Create(createChangeInValueModel.FundAmount.FundId),
                Amount = createChangeInValueModel.FundAmount.Amount
            }
        });
        changeInValueRepository.Add(newChangeInValue);
        await unitOfWork.SaveChangesAsync();
        return Ok(new ChangeInValueModel(newChangeInValue));
    }
}