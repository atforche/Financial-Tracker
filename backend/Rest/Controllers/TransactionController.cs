using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
    TransactionService transactionService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountMapper accountMapper,
    FundAmountMapper fundAmountMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateTransactionModel createTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(createTransactionModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createTransactionModel.AccountingPeriodId), [$"Accounting Period with ID {createTransactionModel.AccountingPeriodId} was not found."]);
        }
        if (!TryBuildCreateTransactionAccountRequest(createTransactionModel.DebitAccount, out CreateTransactionAccountRequest? debitAccountRequest, out Dictionary<string, string[]> debitAccountErrors))
        {
            foreach (KeyValuePair<string, string[]> error in debitAccountErrors)
            {
                errors.Add($"{nameof(createTransactionModel.DebitAccount)}.{error.Key}", error.Value);
            }
        }
        if (!TryBuildCreateTransactionAccountRequest(createTransactionModel.CreditAccount, out CreateTransactionAccountRequest? creditAccountRequest, out Dictionary<string, string[]> creditAccountErrors))
        {
            foreach (KeyValuePair<string, string[]> error in creditAccountErrors)
            {
                errors.Add($"{nameof(createTransactionModel.CreditAccount)}.{error.Key}", error.Value);
            }
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionService.TryCreate(
            new CreateTransactionRequest
            {
                AccountingPeriod = accountingPeriod,
                Date = createTransactionModel.Date,
                Location = createTransactionModel.Location,
                Description = createTransactionModel.Description,
                DebitAccount = debitAccountRequest,
                CreditAccount = creditAccountRequest,
            },
            out Transaction? newTransaction,
            out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(exception => exception switch
            {
                InvalidAccountingPeriodException => nameof(createTransactionModel.AccountingPeriodId),
                InvalidTransactionDateException => nameof(createTransactionModel.Date),
                InvalidDebitAccountException => nameof(createTransactionModel.DebitAccount),
                InvalidCreditAccountException => nameof(createTransactionModel.CreditAccount),
                _ => string.Empty
            }).ToDictionary(group => group.Key, group => group.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionMapper.ToModel(newTransaction));
    }

    /// <summary>
    /// Updates the provided Transaction with the provided properties
    /// </summary>
    [HttpPost("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid transactionId, UpdateTransactionModel updateTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (!TryBuildUpdateTransactionAccountRequest(updateTransactionModel.DebitAccount, out UpdateTransactionAccountRequest? debitAccountRequest, out Dictionary<string, string[]> debitAccountErrors))
        {
            foreach (KeyValuePair<string, string[]> error in debitAccountErrors)
            {
                errors.Add($"{nameof(updateTransactionModel.DebitAccount)}.{error.Key}", error.Value);
            }
        }
        if (!TryBuildUpdateTransactionAccountRequest(updateTransactionModel.CreditAccount, out UpdateTransactionAccountRequest? creditAccountRequest, out Dictionary<string, string[]> creditAccountErrors))
        {
            foreach (KeyValuePair<string, string[]> error in creditAccountErrors)
            {
                errors.Add($"{nameof(updateTransactionModel.CreditAccount)}.{error.Key}", error.Value);
            }
        }
        if (errors.Count > 0 || transaction == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
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
            errors = exceptions.GroupBy(exception => exception switch
            {
                InvalidTransactionDateException => nameof(updateTransactionModel.Date),
                InvalidDebitAccountException => nameof(updateTransactionModel.DebitAccount),
                InvalidCreditAccountException => nameof(updateTransactionModel.CreditAccount),
                _ => string.Empty
            }).ToDictionary(group => group.Key, group => group.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionMapper.ToModel(transaction));
    }

    /// <summary>
    /// Posts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/post")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PostAsync(Guid transactionId, PostTransactionModel postTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (!accountMapper.TryToDomain(postTransactionModel.AccountId, out Account? account))
        {
            errors.Add(nameof(postTransactionModel.AccountId), new[] { $"Account with ID {postTransactionModel.AccountId} was not found." });
        }
        if (errors.Count > 0 || transaction == null || account == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to post Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionService.TryPost(
                transaction,
                account.Id,
                postTransactionModel.Date,
                out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(exception => exception switch
            {
                InvalidTransactionDateException => nameof(postTransactionModel.Date),
                InvalidDebitAccountException or InvalidCreditAccountException => nameof(postTransactionModel.AccountId),
                _ => string.Empty
            }).ToDictionary(group => group.Key, group => group.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to post Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionMapper.ToModel(transaction));
    }

    /// <summary>
    /// Deletes the Transaction with the provided ID
    /// </summary>
    [HttpDelete("{transactionId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid transactionId)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionService.TryDelete(transaction, null, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Transaction.",
                Errors = {
                    { string.Empty, exceptions.Select(e => e.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
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
        out Dictionary<string, string[]> errors)
    {
        request = null;
        errors = new Dictionary<string, string[]>();

        if (model == null)
        {
            return true;
        }
        if (!accountMapper.TryToDomain(model.AccountId, out Account? account))
        {
            errors.Add(nameof(model.AccountId), new[] { $"Account with ID {model.AccountId} was not found." });
        }
        List<FundAmount> fundAmounts = [];
        foreach ((int index, CreateFundAmountModel fundAmountModel) in model.FundAmounts.Index())
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
            {
                errors.Add($"{nameof(model.FundAmounts)}[{index}]", new[] { $"Fund with ID {fundAmountModel.FundId} was not found." });
            }
            else
            {
                fundAmounts.Add(fundAmount);
            }
        }
        if (errors.Count > 0 || account == null)
        {
            return false;
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
        out Dictionary<string, string[]> errors)
    {
        request = null;
        errors = new Dictionary<string, string[]>();

        if (model == null)
        {
            return true;
        }
        List<FundAmount> fundAmounts = [];
        foreach ((int index, CreateFundAmountModel fundAmountModel) in model.FundAmounts.Index())
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
            {
                errors.Add($"{nameof(model.FundAmounts)}[{index}]", new[] { $"Fund with ID {fundAmountModel.FundId} was not found." });
            }
            else
            {
                fundAmounts.Add(fundAmount);
            }
        }
        if (errors.Count > 0)
        {
            return false;
        }
        request = new UpdateTransactionAccountRequest
        {
            FundAmounts = fundAmounts
        };
        return true;
    }
}