using Cinedex.Application.Abstractions.Authentication;

namespace Cinedex.Application.Authentication.Refresh;

public sealed class RefreshUseCase(ITokenProvider tokenProvider)
{
    public async Task<RefreshResponse> HandleAsync(RefreshCommand command, CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);

        // TODO - find user by refresh token
        // TODO - validate refresh token and throw specific exceptions:
        //   - RefreshTokenNotFoundException: token doesn't exist in database → 401
        //   - RefreshTokenRevokedException: token was explicitly revoked/invalidated → 401
        //   - RefreshTokenExpiredException: token exceeded its lifetime → 401
        //   - InvalidRefreshTokenException: malformed, reuse detected, or validation failure → 401
        //   Security: All exceptions return same generic message to client ("Invalid refresh token")
        //   Global exception handler will catch AuthenticationException base type and return 401
        //   Log specific exception types server-side for security monitoring and debugging

        // this will come from the user lookup
        var userId = Guid.NewGuid();
        var email = "email";
        // TODO - generate new access token and refresh token
        var accessToken = tokenProvider.CreateAccessToken(userId, email);
        var refreshToken = tokenProvider.CreateRefreshToken();
        
        // TODO - invalidate existing refresh token (token rotation best practice)
        //   Security: When issuing a new refresh token, invalidate the old one
        //   Consider adding ReplacedByToken field to track token rotation chains
        //   Monitor for refresh token reuse attempts (possible token theft indicator)

        // Database Considerations:
        //   - Store: RefreshToken (string), UserId (Guid), ExpiresAt (DateTime), CreatedAt (DateTime), IsRevoked (bool)
        //   - Consider adding: ReplacedByToken (string, nullable) for tracking rotation chains
        //   - Add index on RefreshToken column for fast lookups
        return new RefreshResponse(accessToken, refreshToken);
    }
}