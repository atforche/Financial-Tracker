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
    FundGoalConverter fundGoalConverter,
    FundGoalGetter fundGoalGetter,
    FundGoalRepository fundGoalRepository,
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
    /// Retrieves the Fund Goals for the Fund that match the specified criteria
    /// </summary>
    [HttpGet("{fundId}/goals")]
    [ProducesResponseType(typeof(CollectionModel<FundGoalModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyGoals(Guid fundId, [FromQuery] FundGoalQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        if (errors.Count > 0 || fund == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund Goals.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundGoalGetter.Get(fund.Id, queryParameters));
    }

    /// <summary>
    /// Retrieves the Fund Goal that matches the provided ID for the provided Fund
    /// </summary>
    [HttpGet("{fundId}/goals/{accountingPeriodId}")]
    [ProducesResponseType(typeof(FundGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetGoal(Guid fundId, Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} not found."]);
        }
        if (!fundGoalConverter.TryToDomain(fundId, accountingPeriodId, out FundGoal? fundGoal))
        {
            errors.Add(nameof(accountingPeriodId), [$"Fund Goal with Accounting Period ID {accountingPeriodId} not found."]);
        }
        if (errors.Count > 0 || fund == null || fundGoal == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundGoalConverter.ToModel(fundGoal));
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
        FundGoalType? goalType = null;
        if (createFundModel.GoalType != null && !FundGoalTypeConverter.TryToDomain(createFundModel.GoalType.Value, out goalType))
        {
            errors.Add(nameof(createFundModel.GoalType), [$"Unrecognized Fund Goal Type: {createFundModel.GoalType}"]);
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
                AccountingPeriod = accountingPeriod,
                GoalType = goalType,
                GoalAmount = createFundModel.GoalAmount,
            },
            out Fund? newFund,
            out FundGoal? newFundGoal,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = GetCreateFundErrors(
                    exceptions,
                    nameof(createFundModel.Name),
                    nameof(createFundModel.AccountingPeriodId),
                    nameof(createFundModel.GoalType),
                    nameof(createFundModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        fundRepository.Add(newFund);
        if (newFundGoal != null)
        {
            fundGoalRepository.Add(newFundGoal);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(newFund));
    }

    /// <summary>
    /// Creates a new Fund Goal for the provided Fund with the provided properties
    /// </summary>
    [HttpPost("{fundId}/goals")]
    [ProducesResponseType(typeof(FundGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateGoalAsync(Guid fundId, CreateFundGoalModel createFundGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!accountingPeriodConverter.TryToDomain(createFundGoalModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createFundGoalModel.AccountingPeriodId), [$"Accounting Period with ID {createFundGoalModel.AccountingPeriodId} was not found."]);
        }
        if (!FundGoalTypeConverter.TryToDomain(createFundGoalModel.GoalType, out FundGoalType? goalType))
        {
            errors.Add(nameof(createFundGoalModel.GoalType), [$"Unrecognized Fund Goal Type: {createFundGoalModel.GoalType}"]);
        }
        if (errors.Count > 0 || fund == null || accountingPeriod == null || goalType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        if (!fundService.TryCreateGoal(
            new CreateFundGoalRequest
            {
                Fund = fund,
                AccountingPeriod = accountingPeriod,
                GoalType = goalType.Value,
                GoalAmount = createFundGoalModel.GoalAmount,
            },
            out FundGoal? newFundGoal,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund Goal.",
                Errors = GetFundGoalErrors(
                    exceptions,
                    nameof(fundId),
                    nameof(createFundGoalModel.AccountingPeriodId),
                    nameof(createFundGoalModel.GoalType),
                    nameof(createFundGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        fundGoalRepository.Add(newFundGoal);
        await unitOfWork.SaveChangesAsync();
        return Ok(fundGoalConverter.ToModel(newFundGoal));
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
    /// Updates the provided Fund Goal with the provided properties
    /// </summary>
    [HttpPost("{fundId}/goals/{accountingPeriodId}")]
    [ProducesResponseType(typeof(FundGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateGoalAsync(Guid fundId, Guid accountingPeriodId, UpdateFundGoalModel updateFundGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!fundGoalConverter.TryToDomain(fundId, accountingPeriodId, out FundGoal? fundGoalToUpdate))
        {
            errors.Add(nameof(accountingPeriodId), [$"Fund Goal with Accounting Period ID {accountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || fund == null || fundGoalToUpdate == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!FundGoalTypeConverter.TryToDomain(updateFundGoalModel.GoalType, out FundGoalType? goalType))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund Goal.",
                Errors = { [nameof(updateFundGoalModel.GoalType)] = new[] { $"Unrecognized Fund Goal Type: {updateFundGoalModel.GoalType}" } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!fundService.TryUpdateGoal(fundGoalToUpdate, goalType.Value, updateFundGoalModel.GoalAmount, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund Goal.",
                Errors = GetFundGoalErrors(
                    exceptions,
                    nameof(fundId),
                    nameof(accountingPeriodId),
                    nameof(updateFundGoalModel.GoalType),
                    nameof(updateFundGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok(fundGoalConverter.ToModel(fundGoalToUpdate));
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

    /// <summary>
    /// Deletes the Fund Goal with the provided Accounting Period ID for the provided Fund
    /// </summary>
    [HttpDelete("{fundId}/goals/{accountingPeriodId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteGoalAsync(Guid fundId, Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!fundGoalConverter.TryToDomain(fundId, accountingPeriodId, out FundGoal? fundGoalToDelete))
        {
            errors.Add(nameof(accountingPeriodId), [$"Fund Goal with Accounting Period ID {accountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || fund == null || fundGoalToDelete == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!fundService.TryDeleteGoal(fundGoalToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund Goal.",
                Errors = {
                    { string.Empty, exceptions.Select(exception => exception.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Maps create Fund exceptions to validation errors
    /// </summary>
    private static Dictionary<string, string[]> GetCreateFundErrors(
        IEnumerable<Exception> exceptions,
        string nameKey,
        string accountingPeriodKey,
        string goalTypeKey,
        string goalAmountKey) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidNameException => nameKey,
            InvalidAccountingPeriodException => accountingPeriodKey,
            InvalidFundGoalTypeException => goalTypeKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            InvalidFundException => string.Empty,
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());

    /// <summary>
    /// Maps Fund Goal exceptions to validation errors
    /// </summary>
    private static Dictionary<string, string[]> GetFundGoalErrors(
        IEnumerable<Exception> exceptions,
        string fundKey,
        string relationshipKey,
        string goalTypeKey,
        string goalAmountKey) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidAccountingPeriodException => relationshipKey,
            InvalidFundGoalTypeException => goalTypeKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("already exists", StringComparison.InvariantCultureIgnoreCase) => relationshipKey,
            InvalidFundException => fundKey,
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
}