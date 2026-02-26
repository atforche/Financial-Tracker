using Data;
using Data.Funds;
using Data.Transactions;
using Domain.Funds;
using Domain.Funds.Exceptions;
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
    TransactionRepository transactionRepository,
    FundService fundService,
    FundMapper fundMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAll([FromQuery] FundQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];

        FundSortOrder? fundSortOrder = null;
        if (queryParameters.SortBy != null && !FundSortOrderMapper.TryToData(queryParameters.SortBy.Value, out fundSortOrder))
        {
            errors.Add(nameof(queryParameters.SortBy), new[] { $"Unrecognized Fund Sort Order Model: {queryParameters.SortBy.Value}" });
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
            SortBy = fundSortOrder,
            Names = queryParameters.Names,
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
    /// Retrieves the Transactions for the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid fundId, [FromQuery] FundTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundMapper.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        FundTransactionSortOrder? fundTransactionSortOrder = null;
        if (queryParameters.SortBy != null && !FundTransactionSortOrderMapper.TryToData(queryParameters.SortBy.Value, out fundTransactionSortOrder))
        {
            errors.Add(nameof(queryParameters.SortBy), new[] { $"Unrecognized Fund Transaction Sort Order Model: {queryParameters.SortBy.Value}" });
        }
        IEnumerable<TransactionType> transactionTypes = [];
        if (queryParameters.Types != null)
        {
            foreach ((int index, TransactionTypeModel transactionTypeModel) in queryParameters.Types.Index())
            {
                if (!TransactionTypeMapper.TryToData(transactionTypeModel, out TransactionType? transactionType))
                {
                    errors.Add($"{nameof(queryParameters.Types)}[{index}]", new[] { $"Unrecognized Transaction Type Model: {transactionTypeModel}" });
                }
                else
                {
                    transactionTypes = transactionTypes.Append(transactionType.Value);
                }
            }
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
            SortBy = fundTransactionSortOrder,
            MinDate = queryParameters.MinDate,
            MaxDate = queryParameters.MaxDate,
            Locations = queryParameters.Locations,
            Types = transactionTypes.Any() ? transactionTypes.ToList() : null,
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
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateOrUpdateFundModel createFundModel)
    {
        if (!fundService.TryCreate(createFundModel.Name, createFundModel.Description, out Fund? newFund, out IEnumerable<Exception> exceptions))
        {
            var errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(createFundModel.Name),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return Ok(fundMapper.ToModel(newFund));
    }

    /// <summary>
    /// Updates the provided Fund with the provided properties
    /// </summary>
    /// <param name="fundId">ID of the Fund to update</param>
    /// <param name="updateFundModel">Request to update a Fund</param>
    /// <returns>The updated Fund</returns>
    [HttpPost("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid fundId, CreateOrUpdateFundModel updateFundModel)
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
    /// Deletes the Fund with the provided ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to delete</param>
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
}