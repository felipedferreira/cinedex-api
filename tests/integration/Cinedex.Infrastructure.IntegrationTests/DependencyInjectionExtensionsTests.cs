using Cinedex.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cinedex.Infrastructure.IntegrationTests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddInfrastructureServices_WithEmptyRequiredJwtOptions_ThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "",
                ["Jwt:Audience"] = "",
                ["Jwt:Secret"] = ""
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructureServices();
        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true
        });
        
        // OptionsValidationException
        var exception = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value);

        // Act & Assert
        Assert.Contains(nameof(JwtOptions.Issuer), exception.Message);
        Assert.Contains(nameof(JwtOptions.Audience), exception.Message);
        Assert.Contains(nameof(JwtOptions.Secret), exception.Message);
    }
}