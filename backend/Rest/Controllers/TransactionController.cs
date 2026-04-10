using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.CreateRequests;
using Domain.Transactions.UpdateRequests;
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
    /// Retrieves the Transaction with the provided ID
    /// </summary>
    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid transactionId)
    {
        if (!transactionMapper.TryToDomain(transactionId, out Transaction? transaction))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Transaction.",
                Errors = {
                    { nameof(transactionId), new[] { $"Transaction with ID {transactionId} was not found." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(transactionMapper.ToModel(transaction));
    }

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

        Account? debitAccount = null;
        if (createTransactionModel.DebitAccount != null && !accountMapper.TryToDomain(createTransactionModel.DebitAccount.AccountId, out debitAccount))
        {
            errors.Add($"{nameof(createTransactionModel.DebitAccount)}.{nameof(createTransactionModel.DebitAccount.AccountId)}",
                [$"Account with ID {createTransactionModel.DebitAccount.AccountId} was not found."]);
        }

        Account? creditAccount = null;
        if (createTransactionModel.CreditAccount != null && !accountMapper.TryToDomain(createTransactionModel.CreditAccount.AccountId, out creditAccount))
        {
            errors.Add($"{nameof(createTransactionModel.CreditAccount)}.{nameof(createTransactionModel.CreditAccount.AccountId)}",
                [$"Account with ID {createTransactionModel.CreditAccount.AccountId} was not found."]);
        }

        List<FundAmount> debitFundAmounts = [];
        if (createTransactionModel.DebitAccount != null)
        {
            foreach ((int index, CreateFundAmountModel fundAmountModel) in createTransactionModel.DebitAccount.FundAmounts.Index())
            {
                if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
                {
                    errors.Add($"{nameof(createTransactionModel.DebitAccount)}.{nameof(createTransactionModel.DebitAccount.FundAmounts)}[{index}]",
                        [$"Fund with ID {fundAmountModel.FundId} was not found."]);
                }
                else
                {
                    debitFundAmounts.Add(fundAmount);
                }
            }
        }

        List<FundAmount> creditFundAmounts = [];
        if (createTransactionModel.CreditAccount != null)
        {
            foreach ((int index, CreateFundAmountModel fundAmountModel) in createTransactionModel.CreditAccount.FundAmounts.Index())
            {
                if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
                {
                    errors.Add($"{nameof(createTransactionModel.CreditAccount)}.{nameof(createTransactionModel.CreditAccount.FundAmounts)}[{index}]",
                        [$"Fund with ID {fundAmountModel.FundId} was not found."]);
                }
                else
                {
                    creditFundAmounts.Add(fundAmount);
                }
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

        CreateTransactionRequest createRequest = BuildCreateRequest(
            accountingPeriod, createTransactionModel, debitAccount, debitFundAmounts, creditAccount, creditFundAmounts);

        if (!transactionService.TryCreate(createRequest, out Transaction? newTransaction, out IEnumerable<Exception> exceptions))
        {
            errors.Add(nameof(CreateTransactionModel.AccountingPeriodId), exceptions.OfType<InvalidAccountingPeriodException>().Select(e => e.Message).ToArray());
            errors.Add(nameof(CreateTransactionModel.Date), exceptions.OfType<InvalidDateException>().Select(e => e.Message).ToArray());
            errors.Add($"{nameof(CreateTransactionModel.DebitAccount)}.{nameof(CreateTransactionAccountModel.AccountId)}",
                exceptions.OfType<InvalidAccountException>().Where(e => e.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
            errors.Add($"{nameof(CreateTransactionModel.DebitAccount)}.{nameof(CreateTransactionAccountModel.FundAmounts)}",
                exceptions.OfType<InvalidFundAmountException>().Where(e => e.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
            errors.Add($"{nameof(CreateTransactionModel.CreditAccount)}.{nameof(CreateTransactionAccountModel.AccountId)}",
                exceptions.OfType<InvalidAccountException>().Where(e => e.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
            errors.Add($"{nameof(CreateTransactionModel.CreditAccount)}.{nameof(CreateTransactionAccountModel.FundAmounts)}",
                exceptions.OfType<InvalidFundAmountException>().Where(e => e.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
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

        List<FundAmount> debitFundAmounts = [];
        if (updateTransactionModel.DebitAccount != null)
        {
            foreach ((int index, CreateFundAmountModel fundAmountModel) in updateTransactionModel.DebitAccount.FundAmounts.Index())
            {
                if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
                {
                    errors.Add($"{nameof(updateTransactionModel.DebitAccount)}.{nameof(updateTransactionModel.DebitAccount.FundAmounts)}[{index}]",
                        [$"Fund with ID {fundAmountModel.FundId} was not found."]);
                }
                else
                {
                    debitFundAmounts.Add(fundAmount);
                }
            }
        }

        List<FundAmount> creditFundAmounts = [];
        if (updateTransactionModel.CreditAccount != null)
        {
            foreach ((int index, CreateFundAmountModel fundAmountModel) in updateTransactionModel.CreditAccount.FundAmounts.Index())
            {
                if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
                {
                    errors.Add($"{nameof(updateTransactionModel.CreditAccount)}.{nameof(updateTransactionModel.CreditAccount.FundAmounts)}[{index}]",
                        [$"Fund with ID {fundAmountModel.FundId} was not found."]);
                }
                else
                {
                    creditFundAmounts.Add(fundAmount);
                }
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

        UpdateTransactionRequest updateRequest = BuildUpdateRequest(transaction, updateTransactionModel, debitFundAmounts);

        if (!transactionService.TryUpdate(transaction, updateRequest, out IEnumerable<Exception> exceptions))
        {
            errors.Add(nameof(UpdateTransactionModel.Date), exceptions.OfType<InvalidDateException>().Select(e => e.Message).ToArray());
            errors.Add($"{nameof(UpdateTransactionModel.DebitAccount)}.{nameof(UpdateTransactionAccountModel.FundAmounts)}",
                exceptions.OfType<InvalidFundAmountException>().Where(e => e.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
            errors.Add($"{nameof(UpdateTransactionModel.CreditAccount)}.{nameof(UpdateTransactionAccountModel.FundAmounts)}",
                exceptions.OfType<InvalidFundAmountException>().Where(e => e.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Message).ToArray());
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
                InvalidDateException => nameof(postTransactionModel.Date),
                InvalidAccountException => nameof(postTransactionModel.AccountId),
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
    /// Unposts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/unpost")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UnpostAsync(Guid transactionId)
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
                Title = "Unable to unpost Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionService.TryUnpost(transaction, out IEnumerable<Exception> exceptions))
        {
            errors.Add("", exceptions.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to unpost Transaction.",
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
    /// Builds the appropriate concrete CreateTransactionRequest subtype based on the provided model
    /// </summary>
    private static CreateTransactionRequest BuildCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateTransactionModel model,
        Account? debitAccount,
        List<FundAmount> debitFundAmounts,
        Account? creditAccount,
        List<FundAmount> creditFundAmounts)
    {
        bool hasDebit = debitAccount != null;
        bool hasCredit = creditAccount != null;
        bool debitHasFunds = debitFundAmounts.Count > 0;
        bool creditHasFunds = creditFundAmounts.Count > 0;

        if (hasDebit && hasCredit && debitHasFunds)
        {
            return new CreateSpendingTransferTransactionRequest
            {
                AccountingPeriodId = accountingPeriod.Id,
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                Amount = model.DebitAccount!.FundAmounts.Sum(fa => fa.Amount),
                Account = debitAccount!,
                PostedDate = model.DebitAccount!.PostedDate,
                FundAmounts = debitFundAmounts,
                CreditAccount = creditAccount!,
                CreditPostedDate = model.CreditAccount!.PostedDate,
            };
        }
        if (hasDebit && hasCredit && creditHasFunds)
        {
            return new CreateIncomeTransferTransactionRequest
            {
                AccountingPeriodId = accountingPeriod.Id,
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                Amount = model.CreditAccount!.FundAmounts.Sum(fa => fa.Amount),
                Account = creditAccount!,
                PostedDate = model.CreditAccount!.PostedDate,
                IsInitialTransactionForAccount = false,
                DebitAccount = debitAccount!,
                DebitPostedDate = model.DebitAccount!.PostedDate,
            };
        }
        if (hasDebit && hasCredit)
        {
            return new CreateTransferTransactionRequest
            {
                AccountingPeriodId = accountingPeriod.Id,
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                Amount = 0,
                DebitAccount = debitAccount!,
                DebitPostedDate = model.DebitAccount!.PostedDate,
                CreditAccount = creditAccount!,
                CreditPostedDate = model.CreditAccount!.PostedDate,
            };
        }
        if (hasDebit)
        {
            return new CreateSpendingTransactionRequest
            {
                AccountingPeriodId = accountingPeriod.Id,
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                Amount = debitFundAmounts.Sum(fa => fa.Amount),
                Account = debitAccount!,
                PostedDate = model.DebitAccount!.PostedDate,
                FundAmounts = debitFundAmounts,
            };
        }
        return new CreateIncomeTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Location = model.Location,
            Description = model.Description,
            Amount = creditFundAmounts.Sum(fa => fa.Amount),
            Account = creditAccount!,
            PostedDate = model.CreditAccount!.PostedDate,
            IsInitialTransactionForAccount = false,
        };
    }

    /// <summary>
    /// Builds the appropriate concrete UpdateTransactionRequest subtype based on the existing transaction type
    /// </summary>
    private static UpdateTransactionRequest BuildUpdateRequest(
        Transaction transaction,
        UpdateTransactionModel model,
        List<FundAmount> debitFundAmounts) => transaction switch
        {
            SpendingTransferTransaction => new UpdateSpendingTransferTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                FundAmounts = debitFundAmounts,
                PostedDate = model.DebitAccount?.PostedDate,
                CreditPostedDate = model.CreditAccount?.PostedDate,
            },
            SpendingTransaction => new UpdateSpendingTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                FundAmounts = debitFundAmounts,
                PostedDate = model.DebitAccount?.PostedDate,
            },
            IncomeTransferTransaction => new UpdateIncomeTransferTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                PostedDate = model.CreditAccount?.PostedDate,
                DebitPostedDate = model.DebitAccount?.PostedDate,
            },
            IncomeTransaction => new UpdateIncomeTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                PostedDate = model.CreditAccount?.PostedDate,
            },
            TransferTransaction => new UpdateTransferTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                DebitPostedDate = model.DebitAccount?.PostedDate,
                CreditPostedDate = model.CreditAccount?.PostedDate,
            },
            RefundTransaction => new UpdateRefundTransactionRequest
            {
                TransactionDate = model.Date,
                Location = model.Location,
                Description = model.Description,
                DebitPostedDate = model.DebitAccount?.PostedDate,
                CreditPostedDate = model.CreditAccount?.PostedDate,
            },
            _ => throw new InvalidOperationException($"Unrecognized transaction type: {transaction.GetType().Name}")
        };
}