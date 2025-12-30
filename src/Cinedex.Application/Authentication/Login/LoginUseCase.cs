using Cinedex.Application.Abstractions.Authentication;

namespace Cinedex.Application.Authentication.Login;

public sealed class LoginUseCase(ITokenProvider tokenProvider)
{
    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        // simulate async work
        await Task.Delay(0, cancellationToken);
        // TODO - we will look up user by email
        // TODO - if user not found, throw exception
        
        // this is a placeholder, if user is found, we will get their real ID
        var userId = Guid.NewGuid();
        
        // TODO - verify user's password
        // TODO - if password invalid, throw exception
        
        var accessToken = tokenProvider.CreateAccessToken(userId, command.Email);
        var refreshToken = tokenProvider.CreateRefreshToken();
        return new LoginResponse(accessToken, refreshToken);
    }
}