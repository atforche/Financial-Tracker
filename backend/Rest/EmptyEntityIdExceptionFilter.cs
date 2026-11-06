using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rest;

/// <summary>
/// Reports a request naming an empty Entity ID as not found, since no entity can have that ID.
/// </summary>
public sealed class EmptyEntityIdExceptionFilter : IExceptionFilter
{
    /// <inheritdoc/>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is EmptyEntityIdException)
        {
            context.Result = new NotFoundResult();
            context.ExceptionHandled = true;
        }
    }
}
