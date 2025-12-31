using System.ComponentModel.DataAnnotations;

namespace Cinedex.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string Secret { get; init; } = string.Empty;
    
    public TimeSpan TokenLifetime { get; init; }
}