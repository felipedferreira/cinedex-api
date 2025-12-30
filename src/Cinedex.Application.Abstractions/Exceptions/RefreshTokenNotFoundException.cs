namespace Cinedex.Application.Abstractions.Exceptions;

/// <summary>
/// Thrown when a refresh token does not exist in the database.
/// Results in 401 Unauthorized - user must re-authenticate.
/// </summary>
public sealed class RefreshTokenNotFoundException : AuthenticationException
{
    public RefreshTokenNotFoundException()
        : base("Invalid refresh token")
    {
    }

    public RefreshTokenNotFoundException(string message)
        : base(message)
    {
    }
}
