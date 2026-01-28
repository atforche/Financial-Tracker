using Domain.AccountingPeriods.Exceptions;
using Domain.Accounts.Exceptions;
using Domain.Funds.Exceptions;
using Models.Errors;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Domain errors to Error Models
/// </summary>
internal sealed class ErrorMapper
{
    /// <summary>
    /// Maps a Domain error to an Error Model
    /// </summary>
    public static ErrorModel ToModel(string message, IEnumerable<Exception> exceptions) =>
        new()
        {
            Message = message,
            Details = exceptions.Select(exception => new ErrorDetailModel
            {
                ErrorCode = MapErrorCode(exception),
                Description = exception.Message
            }).ToList()
        };

    /// <summary>
    /// Maps the provided exception to an Error Code
    /// </summary>
    private static ErrorCode MapErrorCode(Exception exception) => exception switch
    {
        InvalidFundNameException => ErrorCode.InvalidFundName,
        InvalidYearException => ErrorCode.InvalidAccountingPeriodYear,
        InvalidMonthException => ErrorCode.InvalidAccountingPeriodMonth,
        InvalidAccountNameException => ErrorCode.InvalidAccountName,
        _ => ErrorCode.Generic
    };
}