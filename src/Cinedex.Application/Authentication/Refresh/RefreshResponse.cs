namespace Cinedex.Application.Authentication.Refresh;

public record RefreshResponse(
    string AccessToken,
    string RefreshToken);