using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Microsoft.AspNetCore.Mvc;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Types to Account Type Models
/// </summary>
internal sealed class AccountTypeMapper
{
    /// <summary>
    /// Attempts to map the provided Account Type to an Account Type Model
    /// </summary>
    public static bool TryToModel(
        AccountType accountType,
        [NotNullWhen(true)] out AccountTypeModel? accountTypeModel,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        accountTypeModel = accountType switch
        {
            AccountType.Standard => AccountTypeModel.Standard,
            AccountType.Debt => AccountTypeModel.Debt,
            _ => null
        };
        if (accountTypeModel == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Account Type: {accountType}", []));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to map the provided Account Type Model to an Account Type
    /// </summary>
    public static bool TryToDomain(
        AccountTypeModel accountTypeModel,
        [NotNullWhen(true)] out AccountType? accountType,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        accountType = accountTypeModel switch
        {
            AccountTypeModel.Standard => AccountType.Standard,
            AccountTypeModel.Debt => AccountType.Debt,
            _ => null
        };
        if (accountType == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Account Type Model: {accountTypeModel}", []));
            return false;
        }
        return true;
    }
}