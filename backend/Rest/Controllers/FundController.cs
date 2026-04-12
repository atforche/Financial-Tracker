using Data;
using Data.AccountingPeriods;
using Data.Funds;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Funds;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    FundRepository fundRepository,
    FundGoalRepository fundGoalRepository,
    AccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    TransactionRepository transactionRepository,
    FundService fundService,
    AccountingPeriodMapper accountingPeriodMapper,
    FundMapper fundMapper,
    FundGoalMapper fundGoalMapper,
    AccountingPeriodFundMapper accountingPeriodFundMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid fundId)
    {
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund.",
                Errors = { [nameof(fundId)] = new[] { $"Fund with ID {fundId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundMapper.ToModel(fund));
    }

    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetMany([FromQuery] FundQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];

        FundSortOrder? fundSortOrder = null;
        if (queryParameters.Sort != null && !FundSortOrderMapper.TryToData(queryParameters.Sort.Value, out fundSortOrder))
        {
            errors.Add(nameof(queryParameters.Sort), new[] { $"Unrecognized Fund Sort Order Model: {queryParameters.Sort.Value}" });
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Funds.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        PaginatedCollection<Fund> funds = fundRepository.GetMany(new GetFundsRequest
        {
            Search = queryParameters.Search,
            Sort = fundSortOrder,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        });
        return Ok(new CollectionModel<FundModel>
        {
            Items = funds.Items.Select(fundMapper.ToModel).ToList(),
            TotalCount = funds.TotalCount
        });
    }

    /// <summary>
    /// Retrieves the Fund as it appeared in the provided Accounting Period
    /// </summary>
    [HttpGet("{fundId}/{accountingPeriodId}")]
    [ProducesResponseType(typeof(AccountingPeriodFundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAccountingPeriodFund(Guid fundId, Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || fund == null || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        AccountingPeriodBalanceHistory? accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
        FundAccountingPeriodBalanceHistory? fundBalanceHistory = accountingPeriodBalanceHistory?.FundBalances.FirstOrDefault(fundHistory => fundHistory.Fund.Id == fund.Id);
        if (fundBalanceHistory == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = new Dictionary<string, string[]>
                {
                    { nameof(fundId), new[] { $"Fund with ID {fundId} not found for Accounting Period with ID {accountingPeriodId}." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountingPeriodFundMapper.ToModel(fundBalanceHistory));
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
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        AccountingPeriodId? accountingPeriodId = null;
        if (queryParameters.AccountingPeriodId != null && !accountingPeriodMapper.TryToDomain(queryParameters.AccountingPeriodId.Value, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(queryParameters.AccountingPeriodId), new[] { $"Accounting Period with ID {queryParameters.AccountingPeriodId.Value} not found." });
        }
        FundTransactionSortOrder? fundTransactionSortOrder = null;
        if (queryParameters.Sort != null && !FundTransactionSortOrderMapper.TryToData(queryParameters.Sort.Value, out fundTransactionSortOrder))
        {
            errors.Add(nameof(queryParameters.Sort), new[] { $"Unrecognized Fund Transaction Sort Order Model: {queryParameters.Sort.Value}" });
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

        PaginatedCollection<Transaction> paginatedResults = transactionRepository.GetManyByFund(fund.Id, new GetFundTransactionsRequest
        {
            AccountingPeriodId = accountingPeriodId,
            Search = queryParameters.Search,
            Sort = fundTransactionSortOrder,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset
        });
        return Ok(new CollectionModel<TransactionModel>
        {
            Items = paginatedResults.Items.Select(transactionMapper.ToModel).ToList(),
            TotalCount = paginatedResults.TotalCount
        });
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
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        FundGoalSortOrder? sort = null;
        if (queryParameters.Sort != null && !FundGoalSortOrderMapper.TryToData(queryParameters.Sort.Value, out sort))
        {
            errors.Add(nameof(queryParameters.Sort), new[] { $"Unrecognized Fund Goal Sort Order Model: {queryParameters.Sort.Value}" });
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

        PaginatedCollection<FundGoal> paginatedResults = fundGoalRepository.GetManyByFund(fund.Id, new GetFundGoalsRequest
        {
            Search = queryParameters.Search,
            Sort = sort,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        });
        return Ok(new CollectionModel<FundGoalModel>
        {
            Items = paginatedResults.Items.Select(fundGoalMapper.ToModel).ToList(),
            TotalCount = paginatedResults.TotalCount,
        });
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
        if (!accountingPeriodMapper.TryToDomain(createFundModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createFundModel.AccountingPeriodId), [$"Accounting Period with ID {createFundModel.AccountingPeriodId} was not found."]);
        }
        if (!FundTypeMapper.TryToDomain(createFundModel.Type, out FundType? fundType))
        {
            errors.Add(nameof(createFundModel.Type), [$"Unrecognized Fund Type: {createFundModel.Type}"]);
        }
        if (errors.Count > 0 || accountingPeriod == null || fundType == null)
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
                Type = fundType.Value,
                Description = createFundModel.Description,
                AccountingPeriod = accountingPeriod,
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
                    nameof(createFundModel.Type),
                    nameof(createFundModel.AccountingPeriodId),
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
        return Ok(fundMapper.ToModel(newFund));
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
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!accountingPeriodMapper.TryToDomain(createFundGoalModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createFundGoalModel.AccountingPeriodId), [$"Accounting Period with ID {createFundGoalModel.AccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || fund == null || accountingPeriod == null)
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
                    nameof(createFundGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        fundGoalRepository.Add(newFundGoal);
        await unitOfWork.SaveChangesAsync();
        return Ok(fundGoalMapper.ToModel(newFundGoal));
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
        if (!fundMapper.TryToDomain(fundId, out Fund? fundToUpdate))
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
        return Ok(fundMapper.ToModel(fundToUpdate));
    }

    /// <summary>
    /// Updates the provided Fund Goal with the provided properties
    /// </summary>
    [HttpPost("{fundId}/goals/{fundGoalId}")]
    [ProducesResponseType(typeof(FundGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateGoalAsync(Guid fundId, Guid fundGoalId, UpdateFundGoalModel updateFundGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!fundGoalMapper.TryToDomain(fundGoalId, out FundGoal? fundGoalToUpdate))
        {
            errors.Add(nameof(fundGoalId), [$"Fund Goal with ID {fundGoalId} was not found."]);
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
        if (fundGoalToUpdate.Fund.Id != fund.Id)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund Goal.",
                Errors = { [nameof(fundGoalId)] = new[] { $"Fund Goal with ID {fundGoalId} does not belong to Fund with ID {fundId}." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!FundService.TryUpdateGoal(fundGoalToUpdate, updateFundGoalModel.GoalAmount, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund Goal.",
                Errors = GetFundGoalErrors(
                    exceptions,
                    nameof(fundId),
                    nameof(fundGoalId),
                    nameof(updateFundGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok(fundGoalMapper.ToModel(fundGoalToUpdate));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    [HttpDelete("{fundId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundMapper.TryToDomain(fundId, out Fund? fundToDelete))
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
    /// Deletes the Fund Goal with the provided ID for the provided Fund
    /// </summary>
    [HttpDelete("{fundId}/goals/{fundGoalId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteGoalAsync(Guid fundId, Guid fundGoalId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
        }
        if (!fundGoalMapper.TryToDomain(fundGoalId, out FundGoal? fundGoalToDelete))
        {
            errors.Add(nameof(fundGoalId), [$"Fund Goal with ID {fundGoalId} was not found."]);
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
        if (fundGoalToDelete.Fund.Id != fund.Id)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund Goal.",
                Errors = { [nameof(fundGoalId)] = new[] { $"Fund Goal with ID {fundGoalId} does not belong to Fund with ID {fundId}." } },
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
        string typeKey,
        string accountingPeriodKey,
        string goalAmountKey) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidNameException => nameKey,
            InvalidAccountingPeriodException => accountingPeriodKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            InvalidFundException => typeKey,
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());

    /// <summary>
    /// Maps Fund Goal exceptions to validation errors
    /// </summary>
    private static Dictionary<string, string[]> GetFundGoalErrors(
        IEnumerable<Exception> exceptions,
        string fundKey,
        string accountingPeriodKey,
        string goalAmountKey) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidAccountingPeriodException => accountingPeriodKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("already exists", StringComparison.InvariantCultureIgnoreCase) => accountingPeriodKey,
            InvalidFundException => fundKey,
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
}