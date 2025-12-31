namespace Cinedex.Web.Extensions;

/// <summary>
/// Extension methods for configuring ProblemDetails services.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds and configures ProblemDetails services with custom factory that includes trace IDs and RFC 7807 compliance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                // Add traceId for request correlation
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                // Add instance path from the request
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;

                // Set type to RFC 7807 compliant URI if not already set
                context.ProblemDetails.Type ??= $"https://httpstatuses.com/{context.ProblemDetails.Status}";
            };
        });

        return services;
    }
}
