namespace Cinedex.Application.Abstractions.Exceptions;

/// <summary>
/// Thrown when a refresh token has been explicitly revoked or invalidated.
/// Results in 401 Unauthorized - user must re-authenticate.
/// Common scenarios: user logged out, security breach detected, token rotation.
/// </summary>
public sealed class RefreshTokenRevokedException : AuthenticationException
{
    public RefreshTokenRevokedException()
        : base("Invalid refresh token")
    {
    }

    public RefreshTokenRevokedException(string message)
        : base(message)
    {
    }
}
