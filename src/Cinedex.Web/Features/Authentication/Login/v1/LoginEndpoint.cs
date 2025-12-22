using System.Net.Mime;
using Cinedex.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Cinedex.Web.Features.Authentication.Login.v1;

public static class LoginEndpoint
{
    internal static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async ([FromBody] LoginRequest request, ITokenProvider tokenProvider, CancellationToken ct) =>
            {
                var userId = Guid.NewGuid();
                var jwt = tokenProvider.GenerateToken(userId, request.Email);
                return Results.Content(jwt, MediaTypeNames.Text.Plain);
            })
            .WithName("Login")
            .WithTags("Authentication");

        return app;
    }
}