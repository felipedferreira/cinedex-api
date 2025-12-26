using System.Net.Mime;
using Cinedex.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Features.Authentication.Login.v1;

public static class LoginEndpoint
{
    internal static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
                [FromBody] LoginRequest request,
                ITokenProvider tokenProvider,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var userId = Guid.NewGuid();
                var jwt = tokenProvider.GenerateToken(userId, request.Email);
                httpContext.Response.Cookies.Append(
                    "REFRESH_TOKEN",
                    "SAMPLE",
                    new CookieOptions
                    {
                        HttpOnly = true, // Prevent access via JavaScript
                        Secure = true,   // Ensure it's only sent over HTTPS
                        SameSite = SameSiteMode.Strict, // Mitigate CSRF attacks we will set this to same site. -> None works
                        Path = "/movie-svc/refresh",   // lowercase & stable
                        MaxAge = TimeSpan.FromDays(7),
                    });
                return Results.Content(jwt, MediaTypeNames.Text.Plain);
            })
            .WithName("Login")
            .WithTags("Authentication");
        
        app.MapPost("/refresh", async (
                [FromBody] LoginRequest request,
                ITokenProvider tokenProvider,
                IAntiforgery antiforgery,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var userId = Guid.NewGuid();
                await antiforgery.ValidateRequestAsync(httpContext);
                // get cookie from request
                if (!httpContext.Request.Cookies.TryGetValue("REFRESH_TOKEN", out var refreshToken))
                {
                    Console.WriteLine("No refresh token found in cookies.");
                }
                var jwt = tokenProvider.GenerateToken(userId, request.Email);
                return Results.Content(jwt, MediaTypeNames.Text.Plain);
            })
            .WithName("RefreshToken")
            .WithTags("Authentication");

        return app;
    }
}