namespace Cinedex.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string CreateAccessToken(Guid userId, string email);
    string CreateRefreshToken();
}