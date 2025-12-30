using System.Net.Mime;
using Cinedex.Application.Abstractions.Authentication;
using Cinedex.Application.Authentication.Login;
using Cinedex.Application.Authentication.Refresh;
using Cinedex.Web.Constants;
using Cinedex.Web.Features.Authentication.Login;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Features.Authentication;

internal static class AuthenticationEndpoints
{
    internal static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/authentication").WithTags("Authentication");

        authGroup.MapPost("/login", async ([FromBody] LoginRequest request, [FromServices] LoginUseCase useCase, HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                var command = new LoginCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                };
                var tokens = await useCase.HandleAsync(command, cancellationToken);
                SetRefreshTokenCookie(httpContext, tokens.RefreshToken);
                return Results.Content(tokens.AccessToken, MediaTypeNames.Text.Plain);
            })
            .WithName("Login")
            .WithSummary("Authenticates a user and returns an access token")
            .WithDescription("Logs in a user with email and password. Returns a JWT access token in the response body and sets an HTTP-only refresh token cookie.")
            .Produces<string>(StatusCodes.Status200OK, contentType: MediaTypeNames.Text.Plain);

        authGroup.MapPost("/refresh", async (
                [FromServices] IAntiforgery antiforgery,
                [FromServices] RefreshUseCase useCase,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                #region [1. Validate CSRF token (security check before any processing) returns 403 if invalid]
                try
                {
                    await antiforgery.ValidateRequestAsync(httpContext);
                }
                catch (AntiforgeryValidationException)
                {
                    // 403 - CSRF validation failed
                    return Results.Forbid();
                }
                #endregion

                #region [2. Extract refresh token from cookie]
                // Check for refresh token cookie
                if (!httpContext.Request.Cookies.TryGetValue(AuthenticationConstants.RefreshTokenCookie, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    // 401 - When session is no longer valid
                    return Results.Unauthorized();
                }
                #endregion

                var command = new RefreshCommand
                {
                    RefreshToken = refreshToken,
                };
                var tokens = await useCase.HandleAsync(command, cancellationToken);
                SetRefreshTokenCookie(httpContext, tokens.RefreshToken);
                return Results.Content(tokens.AccessToken, MediaTypeNames.Text.Plain);
            })
            .WithName("RefreshToken")
            .WithSummary("Refreshes an access token using a refresh token cookie")
            .WithDescription("Uses the refresh token from the HTTP-only cookie and XSRF token to generate a new access token. Requires X-XSRF-TOKEN header.")
            .Produces<string>(StatusCodes.Status200OK, contentType: MediaTypeNames.Text.Plain)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken)
    {
        httpContext.Response.Cookies.Append(
            AuthenticationConstants.RefreshTokenCookie,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true, // Prevents access via JavaScript
                Secure = true,   // Ensure it's only sent over HTTPS
                SameSite = SameSiteMode.Strict, // Mitigate CSRF attacks we will set this to same site.
                Path = "/authentication/refresh", // Cookie is only sent to the refresh endpoint
                MaxAge = TimeSpan.FromDays(7),
            });
    }
}
