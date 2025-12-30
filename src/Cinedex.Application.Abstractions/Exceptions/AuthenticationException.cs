namespace Cinedex.Application.Abstractions.Exceptions;

/// <summary>
/// Base exception for authentication-related errors.
/// Used for global exception handling to map to appropriate HTTP responses.
/// </summary>
public abstract class AuthenticationException : Exception
{
    protected AuthenticationException(string message) : base(message)
    {
    }

    protected AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
