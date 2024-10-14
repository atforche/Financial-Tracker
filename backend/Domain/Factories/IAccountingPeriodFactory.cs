using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of an Accounting Period
/// </summary>
public interface IAccountingPeriodFactory
{
    /// <summary>
    /// Recreates an existing Accounting Period with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate an Accounting Period</param>
    /// <returns>The recreated Accounting Period</returns>
    AccountingPeriod Recreate(IRecreateAccountingPeriodRequest request);
}

/// <summary>
/// Interface representing a request to recreate an existing Accounting Period
/// </summary>
public interface IRecreateAccountingPeriodRequest
{
    /// <summary>
    /// Id for this Accounting Period
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    int Year { get; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    int Month { get; }

    /// <summary>
    /// Is open flag for this Accounting Period
    /// </summary>
    bool IsOpen { get; }
}