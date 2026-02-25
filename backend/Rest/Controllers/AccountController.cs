using Data;
using Data.Accounts;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Exceptions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    AccountRepository accountRepository,
    TransactionRepository transactionRepository,
    AccountService accountService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountMapper accountMapper,
    FundAmountMapper fundAmountMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>The collection of all Accounts</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetAll([FromQuery] AccountQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];

        AccountSortOrder? accountSortOrder = null;
        if (queryParameters.SortBy != null && !AccountSortOrderMapper.TryToData(queryParameters.SortBy.Value, out accountSortOrder))
        {
            errors.Add(nameof(queryParameters.SortBy), [$"Unrecognized Sort Order: {queryParameters.SortBy.Value}"]);
        }
        IEnumerable<AccountType> accountTypes = [];
        if (queryParameters.Types != null)
        {
            accountTypes = [];
            foreach ((int index, AccountTypeModel accountTypeModel) in queryParameters.Types.Index())
            {
                if (!AccountTypeMapper.TryToDomain(accountTypeModel, out AccountType? accountType))
                {
                    errors.Add($"{nameof(queryParameters.Types)}[{index}]", [$"Unrecognized Account Type: {accountTypeModel}"]);
                }
                else
                {
                    accountTypes = accountTypes.Append(accountType.Value);
                }
            }
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounts.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        List<AccountModel> results = [];
        PaginatedCollection<Account> paginatedResults = accountRepository.GetMany(new GetAccountsRequest
        {
            SortBy = accountSortOrder,
            Names = queryParameters.Names,
            Types = accountTypes.Any() ? accountTypes.ToList() : null,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        });
        foreach (Account account in paginatedResults.Items)
        {
            if (!accountMapper.TryToModel(account, out AccountModel? accountModel))
            {
                return Problem(title: $"Failed to map Account with ID {account.Id} to Account Model.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            results.Add(accountModel);
        }
        return Ok(new CollectionModel<AccountModel>
        {
            Items = results,
            TotalCount = paginatedResults.TotalCount
        });
    }

    /// <summary>
    /// Retrieves the Transactions for the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountId, [FromQuery] AccountTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountMapper.TryToDomain(accountId, out Account? account))
        {
            errors.Add(nameof(accountId), [$"Account with ID {accountId} was not found."]);
        }
        AccountTransactionSortOrder? accountTransactionSortOrder = null;
        if (queryParameters.SortBy != null && !AccountTransactionSortOrderMapper.TryToData(queryParameters.SortBy.Value, out accountTransactionSortOrder))
        {
            errors.Add(nameof(queryParameters.SortBy), [$"Unrecognized Sort Order: {queryParameters.SortBy.Value}"]);
        }
        IEnumerable<TransactionType> transactionTypes = [];
        if (queryParameters.Types != null)
        {
            foreach ((int index, TransactionTypeModel transactionTypeModel) in queryParameters.Types.Index())
            {
                if (!TransactionTypeMapper.TryToData(transactionTypeModel, out TransactionType? transactionType))
                {
                    errors.Add($"{nameof(queryParameters.Types)}[{index}]", [$"Unrecognized Transaction Type: {transactionTypeModel}"]);
                }
                else
                {
                    transactionTypes = transactionTypes.Append(transactionType.Value);
                }
            }
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

        PaginatedCollection<Transaction> paginatedResults = transactionRepository.GetManyByAccount(account.Id, new GetAccountTransactionsRequest
        {
            SortBy = accountTransactionSortOrder,
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
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(createAccountModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createAccountModel.AccountingPeriodId), [$"Accounting Period with ID {createAccountModel.AccountingPeriodId} was not found."]);
        }
        if (!AccountTypeMapper.TryToDomain(createAccountModel.Type, out AccountType? accountType))
        {
            errors.Add(nameof(createAccountModel.Type), [$"Unrecognized Account Type: {createAccountModel.Type}"]);
        }
        List<FundAmount> fundAmounts = [];
        foreach ((int index, CreateFundAmountModel fundAmountModel) in createAccountModel.InitialFundAmounts.Index())
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
            {
                errors.Add($"{nameof(createAccountModel.InitialFundAmounts)}[{index}]", [$"Fund with ID {fundAmountModel.FundId} was not found."]);
            }
            else
            {
                fundAmounts.Add(fundAmount);
            }
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
                AccountingPeriod = accountingPeriod,
                AddDate = createAccountModel.AddDate,
                InitialFundAmounts = fundAmounts
            },
            out Account? newAccount,
            out Transaction? initialTransaction,
            out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(createAccountModel.Name),
                InvalidAccountingPeriodException => nameof(createAccountModel.AccountingPeriodId),
                InvalidAddDateException => nameof(createAccountModel.AddDate),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        accountRepository.Add(newAccount);
        if (initialTransaction != null)
        {
            transactionRepository.Add(initialTransaction);
        }
        if (!accountMapper.TryToModel(newAccount, out AccountModel? accountModel))
        {
            return Problem(title: $"Failed to map Account with ID {newAccount.Id} to Account Model.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountModel);
    }

    /// <summary>
    /// Updates the provided Account with the provided properties
    /// </summary>
    /// <param name="accountId">ID of the Account to update</param>
    /// <param name="updateAccountModel">Request to update an Account</param>
    /// <returns>The updated Account</returns>
    [HttpPost("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? accountToUpdate))
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
        if (!accountMapper.TryToModel(accountToUpdate, out AccountModel? accountModel))
        {
            return Problem(title: $"Failed to map Account with ID {accountToUpdate.Id} to Account Model.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountModel);
    }

    /// <summary>
    /// Deletes the Account with the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to delete</param>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountId)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? accountToDelete))
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