namespace Cinedex.Web.Features.Authentication.Login.v1;

public sealed record LoginRequest(
    string Email,
    string Password
);