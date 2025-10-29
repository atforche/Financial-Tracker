using System.Diagnostics.CodeAnalysis;
using Data;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Funds;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    IFundRepository fundRepository,
    FundService fundService) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Funds from the database
    /// </summary>
    /// <returns>A collection of all Funds</returns>
    [HttpGet("")]
    public IReadOnlyCollection<FundModel> GetAll() => fundRepository.FindAll().Select(FundMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    /// <param name="fundId">Id of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public IActionResult Get(Guid fundId)
    {
        if (!TryFindById(fundId, out Fund? fund, out IActionResult? errorResult))
        {
            return errorResult!;
        }
        return Ok(FundMapper.ToModel(fund));
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateOrUpdateFundModel createFundModel)
    {
        if (!fundService.TryCreate(createFundModel.Name, createFundModel.Description, out Fund? newFund, out List<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Fund.", exceptions));
        }
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return Ok(FundMapper.ToModel(newFund));
    }

    /// <summary>
    /// Updates the provided Fund with the provided properties
    /// </summary>
    /// <param name="fundId">ID of the Fund to update</param>
    /// <param name="updateFundModel">Request to update a Fund</param>
    /// <returns>The updated Fund</returns>
    [HttpPost("{fundId}")]
    public async Task<IActionResult> UpdateAsync(Guid fundId, CreateOrUpdateFundModel updateFundModel)
    {
        if (!TryFindById(fundId, out Fund? fundToUpdate, out IActionResult? errorResult))
        {
            return errorResult!;
        }
        if (!fundService.TryUpdate(fundToUpdate, updateFundModel.Name, updateFundModel.Description, out List<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to update Fund.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(FundMapper.ToModel(fundToUpdate));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to delete</param>
    [HttpDelete("{fundId}")]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        if (!TryFindById(fundId, out Fund? fundToDelete, out IActionResult? errorResult))
        {
            return errorResult!;
        }
        if (!fundService.TryDelete(fundToDelete, out List<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Fund.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Attempts to find the Fund with the specified ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to find</param>
    /// <param name="fund">Fund that was found</param>
    /// <param name="errorResult">Result to return if the fund was not found</param>
    /// <returns>True if the fund was found, false otherwise</returns>
    private bool TryFindById(Guid fundId, [NotNullWhen(true)] out Fund? fund, [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!fundRepository.TryFindById(fundId, out fund))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Fund with ID {fundId} was not found.", Array.Empty<Exception>()));
        }
        return errorResult == null;
    }
}