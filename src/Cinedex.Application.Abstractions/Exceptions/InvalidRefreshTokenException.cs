namespace Cinedex.Application.Abstractions.Exceptions;

/// <summary>
/// Thrown when a refresh token is invalid for reasons other than not found, expired, or revoked.
/// Examples: malformed token, token reuse detected (possible theft), cryptographic validation failure.
/// Results in 401 Unauthorized - user must re-authenticate.
/// </summary>
public sealed class InvalidRefreshTokenException : AuthenticationException
{
    public InvalidRefreshTokenException()
        : base("Invalid refresh token")
    {
    }

    public InvalidRefreshTokenException(string message)
        : base(message)
    {
    }
}
