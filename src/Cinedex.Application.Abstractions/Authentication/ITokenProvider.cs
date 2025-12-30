namespace Cinedex.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string GenerateAccessToken(Guid userId, string email);
}