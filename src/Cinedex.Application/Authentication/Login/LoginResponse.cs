namespace Cinedex.Application.Authentication.Login;

public record LoginResponse(
    string AccessToken,
    string RefreshToken);