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
    /// <param name="message">Message for the Error Model</param>
    /// <param name="exceptions">Exceptions to include in the Error Model details</param>
    /// <returns>The mapped Error Model</returns>
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
    /// <param name="exception">Exception to map</param>
    /// <returns>The mapped Error Code</returns>
    private static ErrorCode MapErrorCode(Exception exception) => exception switch
    {
        InvalidFundNameException => ErrorCode.InvalidFundName,
        _ => ErrorCode.Generic
    };
}