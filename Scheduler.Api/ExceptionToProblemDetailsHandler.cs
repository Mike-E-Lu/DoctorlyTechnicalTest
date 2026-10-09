using Scheduler.Application;
using Scheduler.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Scheduler.Api;

/// <summary>Maps known exceptions to RFC 7807 problem responses; anything else becomes a 500.</summary>
public class ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Invalid request"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, "")
        };
        if (status == 0) return false;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = { Status = status, Title = title, Detail = exception.Message }
        });
    }
}
