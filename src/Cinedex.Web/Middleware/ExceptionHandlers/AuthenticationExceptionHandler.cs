using Cinedex.Application.Abstractions.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Middleware.ExceptionHandlers;

/// <summary>
/// Exception handler for authentication-related exceptions.
/// Returns 401 Unauthorized with ProblemDetails response.
/// </summary>
/// <param name="logger">Logger for recording authentication failures.</param>
public sealed class AuthenticationExceptionHandler(ILogger<AuthenticationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AuthenticationException authException)
        {
            return false; // Not handled, pass to next handler
        }

        // Log specific exception type for security monitoring
        // This provides detailed information server-side while returning generic message to client
        logger.LogWarning(
            authException,
            "Authentication failed: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Invalid refresh token", // Generic message for security (prevents user enumeration)
            Type = "https://httpstatuses.com/401"
        };

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception handled
    }
}
