using Data;
using Data.Funds;
using Data.Transactions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Errors;
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
    [ProducesResponseType(typeof(IReadOnlyCollection<FundModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAll([FromQuery] FundQueryParameterModel queryParameters)
    {
        FundSortOrder? fundSortOrder = null;
        if (queryParameters.SortBy != null && !FundSortOrderMapper.TryToData(queryParameters.SortBy.Value, out fundSortOrder, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(fundRepository.GetMany(new GetFundsRequest
        {
            SortBy = fundSortOrder,
            Names = queryParameters.Names,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        }).Select(fundMapper.ToModel).ToList());
    }

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public IActionResult Get(Guid fundId)
    {
        if (!fundMapper.TryToDomain(fundId, out Fund? fund, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(fundMapper.ToModel(fund));
    }

    /// <summary>
    /// Retrieves the Transactions for the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}/transactions")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid fundId)
    {
        if (!fundMapper.TryToDomain(fundId, out Fund? fund, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(transactionRepository.GetManyByFund(fund.Id, new()).Select(transactionMapper.ToModel).ToList());
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateOrUpdateFundModel createFundModel)
    {
        if (!fundService.TryCreate(createFundModel.Name, createFundModel.Description, out Fund? newFund, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Fund.", exceptions));
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
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid fundId, CreateOrUpdateFundModel updateFundModel)
    {
        if (!fundMapper.TryToDomain(fundId, out Fund? fundToUpdate, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!fundService.TryUpdate(fundToUpdate, updateFundModel.Name, updateFundModel.Description, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to update Fund.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundMapper.ToModel(fundToUpdate));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to delete</param>
    [HttpDelete("{fundId}")]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        if (!fundMapper.TryToDomain(fundId, out Fund? fundToDelete, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!fundService.TryDelete(fundToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Fund.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}