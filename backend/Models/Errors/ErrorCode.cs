namespace Models.Errors;

/// <summary>
/// Enum representing the different error codes that the API can return
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Generic error code
    /// </summary>
    Generic,

    /// <summary>
    /// Invalid Fund Name
    /// </summary>
    InvalidFundName,

    /// <summary>
    /// Invalid Accounting Period Year
    /// </summary>
    InvalidAccountingPeriodYear,

    /// <summary>
    /// Invalid Accounting Period Month
    /// </summary>
    InvalidAccountingPeriodMonth,

    /// <summary>
    /// Invalid Account Name
    /// </summary>
    InvalidAccountName,
}