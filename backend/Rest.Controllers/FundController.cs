using Data;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Funds;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    IFundRepository fundRepository,
    FundIdFactory fundIdFactory,
    FundService fundService) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Funds from the database
    /// </summary>
    /// <returns>A collection of all Funds</returns>
    [HttpGet("")]
    public IReadOnlyCollection<FundModel> GetAll() =>
        fundRepository.FindAll().Select(fund => new FundModel(fund)).ToList();

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    /// <param name="fundId">Id of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public FundModel Get(Guid fundId)
    {
        FundId id = fundIdFactory.Create(fundId);
        return new FundModel(fundRepository.FindById(id));
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    public async Task<FundModel> CreateAsync(CreateOrUpdateFundModel createFundModel)
    {
        Fund newFund = fundService.Create(createFundModel.Name, createFundModel.Description);
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return new FundModel(newFund);
    }

    /// <summary>
    /// Updates the provided Fund with the provided properties
    /// </summary>
    /// <param name="fundId">ID of the Fund to update</param>
    /// <param name="updateFundModel">Request to update a Fund</param>
    /// <returns>The updated Fund</returns>
    [HttpPost("{fundId}")]
    public async Task<FundModel> UpdateAsync(Guid fundId, CreateOrUpdateFundModel updateFundModel)
    {
        FundId id = fundIdFactory.Create(fundId);
        Fund fundToUpdate = fundRepository.FindById(id);
        fundToUpdate = fundService.Update(fundToUpdate, updateFundModel.Name, updateFundModel.Description);
        await unitOfWork.SaveChangesAsync();
        return new FundModel(fundToUpdate);
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    /// <param name="fundId">ID of the Fund to delete</param>
    [HttpDelete("{fundId}")]
    public async Task DeleteAsync(Guid fundId)
    {
        FundId id = fundIdFactory.Create(fundId);
        Fund fundToDelete = fundRepository.FindById(id);
        fundRepository.Delete(fundToDelete);
        await unitOfWork.SaveChangesAsync();
    }
}