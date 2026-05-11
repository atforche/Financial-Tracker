using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Accounts;
using Models.Transactions;
using Rest.AccountingPeriods;

namespace Rest.Accounts;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    AccountingPeriodConverter accountingPeriodConverter,
    AccountService accountService,
    AccountGetter accountGetter,
    AccountConverter accountConverter,
    AccountTransactionGetter accountTransactionGetter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid accountId)
    {
        if (!accountConverter.TryToDomain(accountId, out Account? account))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account.",
                Errors = { [nameof(accountId)] = new[] { $"Account with ID {accountId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(accountConverter.ToModel(account));
    }

    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetMany([FromQuery] AccountQueryParameterModel queryParameters) =>
        Ok(accountGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves the Transactions for the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountId, [FromQuery] AccountTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountConverter.TryToDomain(accountId, out Account? account))
        {
            errors.Add(nameof(accountId), [$"Account with ID {accountId} was not found."]);
        }
        if (errors.Count > 0 || account == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account Transactions.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountTransactionGetter.Get(account.Id, queryParameters));
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(createAccountModel.OpeningAccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createAccountModel.OpeningAccountingPeriodId), [$"Accounting Period with ID {createAccountModel.OpeningAccountingPeriodId} was not found."]);
        }
        if (!AccountTypeConverter.TryToDomain(createAccountModel.Type, out AccountType? accountType))
        {
            errors.Add(nameof(createAccountModel.Type), [$"Unrecognized Account Type: {createAccountModel.Type}"]);
        }
        if (errors.Count > 0 || accountingPeriod == null || accountType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        if (!accountService.TryCreate(
            new CreateAccountRequest
            {
                Name = createAccountModel.Name,
                Type = accountType.Value,
                OpeningAccountingPeriod = accountingPeriod,
                DateOpened = createAccountModel.DateOpened
            },
            out Account? newAccount,
            out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(createAccountModel.Name),
                InvalidAccountingPeriodException => nameof(createAccountModel.OpeningAccountingPeriodId),
                InvalidDateException => nameof(createAccountModel.DateOpened),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountConverter.ToModel(newAccount));
    }

    /// <summary>
    /// Updates the provided Account with the provided properties
    /// </summary>
    [HttpPost("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        if (!accountConverter.TryToDomain(accountId, out Account? accountToUpdate))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Account.",
                Errors = {
                    { nameof(accountId), [$"Account with ID {accountId} was not found."]}
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountService.TryUpdate(accountToUpdate, updateAccountModel.Name, out IEnumerable<Exception> exceptions))
        {
            var errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(updateAccountModel.Name),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountConverter.ToModel(accountToUpdate));
    }

    /// <summary>
    /// Deletes the Account with the provided ID
    /// </summary>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountId)
    {
        if (!accountConverter.TryToDomain(accountId, out Account? accountToDelete))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Account.",
                Errors = {
                    { nameof(accountId), [$"Account with ID {accountId} was not found."]}
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountService.TryDelete(accountToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Account.",
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