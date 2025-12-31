using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Middleware.ExceptionHandlers;

/// <summary>
/// Exception handler for validation-related exceptions.
/// Returns 400 Bad Request with ValidationProblemDetails response.
/// </summary>
/// <remarks>
/// This is currently a placeholder for future FluentValidation integration.
/// Handles System.ComponentModel.DataAnnotations.ValidationException for now.
/// </remarks>
/// <param name="logger">Logger for recording validation failures.</param>
public sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false; // Not handled, pass to next handler
        }

        logger.LogWarning(
            validationException,
            "Validation failed: {Message}",
            validationException.Message);

        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Type = "https://httpstatuses.com/400"
        };

        // Add validation error details if available
        if (!string.IsNullOrEmpty(validationException.Message))
        {
            problemDetails.Errors.Add("validation", new[] { validationException.Message });
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception handled
    }
}
