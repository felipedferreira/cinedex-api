namespace Cinedex.Application.Abstractions.Exceptions;

/// <summary>
/// Thrown when a refresh token has exceeded its lifetime.
/// Results in 401 Unauthorized - user must re-authenticate.
/// </summary>
public sealed class RefreshTokenExpiredException : AuthenticationException
{
    public RefreshTokenExpiredException()
        : base("Invalid refresh token")
    {
    }

    public RefreshTokenExpiredException(string message)
        : base(message)
    {
    }
}
