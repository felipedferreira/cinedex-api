using System.ComponentModel.DataAnnotations;

namespace Cinedex.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required]
    public string Issuer { get; init; }
    
    [Required]
    public string Audience { get; init; }
    
    [Required]
    public string Secret { get; init; }
    
    public TimeSpan TokenLifetime { get; init; }
}