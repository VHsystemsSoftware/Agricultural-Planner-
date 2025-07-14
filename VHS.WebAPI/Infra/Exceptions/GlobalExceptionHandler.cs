using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace VHS.WebAPI.Infra.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    private static readonly ValueTask<bool> IsHandled = new(true);
    private static readonly ValueTask<bool> IsNotHandled = new(false);

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!IsCancelationException(exception)) return IsNotHandled;

        _logger.LogDebug(exception, "Ignoring cancellation exception.");

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return IsHandled;
    }

    private static bool IsCancelationException(Exception exception)
    {
        var operationCanceledException = exception as OperationCanceledException;
        if (operationCanceledException is not null)
        {
            // HttpClient timeouts are propagated as OperationCanceledExceptions.
            // This is by design, but we do want to see these errors in the logs since it
            // shows a possible network issue.
            return !operationCanceledException.Message.Contains("HttpClient");
        }

        // When running EF queries, sometimes cancelations will be thrown as InvalidOperationException.
        // And sometimes Microsoft.Data.SqlClient will throw SqlException when a query is cancelled.
        // https://github.com/dotnet/SqlClient/issues/26
        if (exception.Message.Contains("Operation cancelled by user"))
        {
            return true;
        }

        if (exception is Microsoft.Data.SqlClient.SqlException sqlException)
        {
            foreach (var error in sqlException.Errors)
            {
                if (error is not Microsoft.Data.SqlClient.SqlError sqlError)
                {
                    continue;
                }

                if (sqlError.Message == "Operation cancelled by user.")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
