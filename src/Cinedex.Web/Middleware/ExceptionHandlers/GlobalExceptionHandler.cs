using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Middleware.ExceptionHandlers;

/// <summary>
/// Global exception handler that catches all unhandled exceptions.
/// Returns 500 Internal Server Error with ProblemDetails response.
/// </summary>
/// <remarks>
/// This is a catch-all handler that should be registered last in the exception handler chain.
/// It provides environment-aware error details (verbose in Development, generic in Production).
/// </remarks>
/// <param name="logger">Logger for recording unhandled exceptions.</param>
/// <param name="environment">Host environment for determining Development vs Production behavior.</param>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log full exception details server-side for debugging and monitoring
        logger.LogError(
            exception,
            "Unhandled exception occurred: {Message}",
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = "https://httpstatuses.com/500"
        };

        // Show detailed information only in Development environment
        if (environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }
        else
        {
            // Generic message in Production to prevent information leakage
            problemDetails.Detail = "An error occurred while processing your request.";
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Always handled (catch-all)
    }
}
