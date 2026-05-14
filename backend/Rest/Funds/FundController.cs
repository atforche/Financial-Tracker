using Data;
using Data.Funds;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Funds;
using Models.Transactions;
using Rest.AccountingPeriods;

namespace Rest.Funds;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    AccountingPeriodConverter accountingPeriodConverter,
    FundConverter fundConverter,
    FundGetter fundGetter,
    FundRepository fundRepository,
    FundService fundService,
    FundTransactionGetter fundTransactionGetter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid fundId)
    {
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund.",
                Errors = { [nameof(fundId)] = new[] { $"Fund with ID {fundId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundConverter.ToModel(fund));
    }

    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    public IActionResult GetMany([FromQuery] FundQueryParameterModel queryParameters) =>
        Ok(fundGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves the unassigned Fund
    /// </summary>
    [HttpGet("unassigned")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetUnassigned()
    {
        Fund? unassignedFund = fundRepository.GetUnassignedFund();
        if (unassignedFund == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund.",
                Errors = { [""] = ["The unassigned fund was not found."] },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundConverter.ToModel(unassignedFund));
    }

    /// <summary>
    /// Retrieves the Transactions for the Fund that match the specified criteria
    /// </summary>
    [HttpGet("{fundId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyTransactions(Guid fundId, [FromQuery] FundTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        AccountingPeriod? accountingPeriod = null;
        if (queryParameters.AccountingPeriodId != null && !accountingPeriodConverter.TryToDomain(queryParameters.AccountingPeriodId.Value, out accountingPeriod))
        {
            errors.Add(nameof(queryParameters.AccountingPeriodId), new[] { $"Accounting Period with ID {queryParameters.AccountingPeriodId.Value} not found." });
        }
        if (errors.Count > 0 || fund == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund Transactions.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(fundTransactionGetter.Get(fund.Id, queryParameters, accountingPeriod?.Id));
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateFundModel createFundModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(createFundModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createFundModel.AccountingPeriodId), [$"Accounting Period with ID {createFundModel.AccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        if (!fundService.TryCreate(
            new CreateFundRequest
            {
                Name = createFundModel.Name,
                Description = createFundModel.Description,
                OpeningAccountingPeriod = accountingPeriod,
            },
            out Fund? newFund,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = exceptions.GroupBy(exception => exception switch
                {
                    InvalidNameException => nameof(createFundModel.Name),
                    InvalidAccountingPeriodException => nameof(createFundModel.AccountingPeriodId),
                    InvalidFundException => string.Empty,
                    _ => string.Empty,
                }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray()),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(newFund));
    }

    /// <summary>
    /// Onboards a new Fund with the provided properties
    /// </summary>
    [HttpPost("onboard")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OnboardAsync(OnboardFundModel onboardFundModel)
    {
        if (!fundService.TryOnboard(
            new OnboardFundRequest
            {
                Name = onboardFundModel.Name,
                Description = onboardFundModel.Description,
                OnboardedBalance = onboardFundModel.OnboardedBalance
            },
            out Fund? newFund,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Fund.",
                Errors = exceptions.GroupBy(exception => exception switch
                {
                    InvalidNameException => nameof(onboardFundModel.Name),
                    InvalidAmountException => nameof(onboardFundModel.OnboardedBalance),
                    InvalidAccountingPeriodException => string.Empty,
                    InvalidFundException => string.Empty,
                    _ => string.Empty,
                }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray()),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(newFund));
    }

    /// <summary>
    /// Updates the provided Fund with the provided properties
    /// </summary>
    [HttpPost("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid fundId, UpdateFundModel updateFundModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fundToUpdate))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!fundService.TryUpdate(fundToUpdate, updateFundModel.Name, updateFundModel.Description, out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(updateFundModel.Name),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(fundToUpdate));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    [HttpDelete("{fundId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fundToDelete))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!fundService.TryDelete(fundToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund.",
                Errors = {
                    { string.Empty, exceptions.Select(e => e.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}