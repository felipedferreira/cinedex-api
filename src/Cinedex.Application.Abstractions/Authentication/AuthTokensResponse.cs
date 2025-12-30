namespace Cinedex.Application.Abstractions.Authentication;

public sealed record AuthTokensResponse(
    string AccessToken,
    string RefreshToken
);
