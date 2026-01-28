using System.Diagnostics.CodeAnalysis;
using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Errors;
using Models.Funds;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Transactions
/// </summary>
[ApiController]
[Route("/transactions")]
public sealed class TransactionController(
    UnitOfWork unitOfWork,
    ITransactionRepository transactionRepository,
    TransactionService transactionService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountMapper accountMapper,
    FundAmountMapper fundAmountMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Transactions from the database
    /// </summary>
    [HttpGet("")]
    public IReadOnlyCollection<TransactionModel> GetAll() => transactionRepository.GetAll().Select(transactionMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves the Transaction that matches the provided ID
    /// </summary>
    [HttpGet("{transactionId}")]
    public IActionResult Get(Guid transactionId)
    {
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(transactionMapper.ToModel(transaction));
    }

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateTransactionModel createTransactionModel)
    {
        if (!accountingPeriodMapper.TryToDomain(createTransactionModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!TryBuildCreateTransactionAccountRequest(createTransactionModel.DebitAccount, out CreateTransactionAccountRequest? debitAccountRequest, out errorResult))
        {
            return errorResult;
        }
        if (!TryBuildCreateTransactionAccountRequest(createTransactionModel.CreditAccount, out CreateTransactionAccountRequest? creditAccountRequest, out errorResult))
        {
            return errorResult;
        }
        if (!transactionService.TryCreate(
            new CreateTransactionRequest
            {
                AccountingPeriod = accountingPeriod.Id,
                Date = createTransactionModel.Date,
                Location = createTransactionModel.Location,
                Description = createTransactionModel.Description,
                DebitAccount = debitAccountRequest,
                CreditAccount = creditAccountRequest,
            },
            out Transaction? newTransaction,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Transaction.", exceptions));
        }
        transactionRepository.Add(newTransaction);
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionMapper.ToModel(newTransaction));
    }

    /// <summary>
    /// Updates the provided Transaction with the provided properties
    /// </summary>
    [HttpPost("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid transactionId, UpdateTransactionModel updateTransactionModel)
    {
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!TryBuildUpdateTransactionAccountRequest(updateTransactionModel.DebitAccount, out UpdateTransactionAccountRequest? debitAccountRequest, out errorResult))
        {
            return errorResult;
        }
        if (!TryBuildUpdateTransactionAccountRequest(updateTransactionModel.CreditAccount, out UpdateTransactionAccountRequest? creditAccountRequest, out errorResult))
        {
            return errorResult;
        }
        if (!transactionService.TryUpdate(
                transaction,
                new UpdateTransactionRequest
                {
                    Date = updateTransactionModel.Date,
                    Location = updateTransactionModel.Location,
                    Description = updateTransactionModel.Description,
                    DebitAccount = debitAccountRequest,
                    CreditAccount = creditAccountRequest
                },
                out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to update Transaction.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionMapper.ToModel(transaction));
    }

    /// <summary>
    /// Deletes the Transaction with the provided ID
    /// </summary>
    [HttpDelete("{transactionId}")]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid transactionId)
    {
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!transactionService.TryDelete(transaction, null, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Transaction.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Attempts to build a CreateTransactionAccountRequest from the provided CreateTransactionAccountModel
    /// </summary>
    private bool TryBuildCreateTransactionAccountRequest(
        CreateTransactionAccountModel? model,
        out CreateTransactionAccountRequest? request,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        request = null;
        errorResult = null;

        if (model == null)
        {
            return true;
        }
        if (!accountMapper.TryToDomain(model.AccountId, out Account? account, out errorResult))
        {
            return false;
        }
        List<FundAmount> fundAmounts = [];
        foreach (CreateFundAmountModel fundAmountModel in model.FundAmounts)
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount, out errorResult))
            {
                return false;
            }
            fundAmounts.Add(fundAmount);
        }
        request = new CreateTransactionAccountRequest
        {
            Account = account,
            FundAmounts = fundAmounts
        };
        return true;
    }

    /// <summary>
    /// Attempts to build a UpdateTransactionAccountRequest from the provided UpdateTransactionAccountModel
    /// </summary>
    private bool TryBuildUpdateTransactionAccountRequest(
        UpdateTransactionAccountModel? model,
        out UpdateTransactionAccountRequest? request,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        request = null;
        errorResult = null;

        if (model == null)
        {
            return true;
        }
        List<FundAmount> fundAmounts = [];
        foreach (CreateFundAmountModel fundAmountModel in model.FundAmounts)
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount, out errorResult))
            {
                return false;
            }
            fundAmounts.Add(fundAmount);
        }
        request = new UpdateTransactionAccountRequest
        {
            FundAmounts = fundAmounts
        };
        return true;
    }
}