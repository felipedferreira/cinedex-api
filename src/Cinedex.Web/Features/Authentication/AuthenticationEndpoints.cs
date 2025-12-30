using System.Net.Mime;
using Cinedex.Application.Abstractions.Authentication;
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

        authGroup.MapPost("/login", ([FromBody] LoginRequest request, ITokenProvider tokenProvider, HttpContext httpContext, CancellationToken ct) =>
            {
                var userId = Guid.NewGuid();
                var accessToken = tokenProvider.GenerateAccessToken(userId, request.Email);
                // TODO: Write code that will generate a refresh token
                var refreshToken = accessToken;
                httpContext.Response.Cookies.Append(
                    AuthenticationConstants.RefreshTokenCookie,
                    refreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true, // Prevents access via JavaScript
                        Secure = true,   // Ensure it's only sent over HTTPS
                        SameSite = SameSiteMode.Strict, // Mitigate CSRF attacks we will set this to same site.
                        Path = $"{PathConstants.ApiBasePath}/auth/refresh", // Cookie is only sent to the refresh endpoint
                        MaxAge = TimeSpan.FromDays(7),
                    });
                return Results.Content(accessToken, MediaTypeNames.Text.Plain);
            })
            .WithName("Login")
            .WithSummary("Authenticates a user and returns an access token")
            .WithDescription("Logs in a user with email and password. Returns a JWT access token in the response body and sets an HTTP-only refresh token cookie.")
            .Produces<string>(StatusCodes.Status200OK, contentType: MediaTypeNames.Text.Plain);

        authGroup.MapPost("/refresh", async (
                IAntiforgery antiforgery,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                // get cookie from request using the const
                if (!httpContext.Request.Cookies.TryGetValue(AuthenticationConstants.RefreshTokenCookie, out var refreshToken))
                {
                    // TODO - consider what to do if no refresh token is found
                    Console.WriteLine("No refresh token found in cookies.");
                }

                // consider what to do if no xsrf header is found
                if (!httpContext.Request.Headers.TryGetValue(AntiforgeryConstants.XsrfHeader, out var xsrfHeader))
                {
                    Console.WriteLine("xsrfHeader not found in headers.");
                }

                // should we validate the refresh token here? should this belong in application layer?
                await antiforgery.ValidateRequestAsync(httpContext);

                return Results.NoContent();
            })
            .WithName("RefreshToken")
            .WithSummary("Refreshes an access token using a refresh token cookie")
            .WithDescription("Uses the refresh token from the HTTP-only cookie and XSRF token to generate a new access token. Requires X-XSRF-TOKEN header.")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
