using System.Text;
using Cinedex.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Cinedex.Infrastructure.Authentication;

public class JwtTokenProvider(IOptions<JwtOptions> jwtOptions) : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public string GenerateAccessToken(Guid userId, string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, email),
            }),
            Expires = DateTime.UtcNow.AddHours(1), // this should be configurable
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
        };

        var handler = new JsonWebTokenHandler();
        var jwt = handler.CreateToken(tokenDescriptor);
        
        return jwt;
    }
}