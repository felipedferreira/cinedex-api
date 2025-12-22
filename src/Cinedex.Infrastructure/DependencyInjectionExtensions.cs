using Cinedex.Application.Abstractions.Authentication;
using Cinedex.Application.Abstractions.Persistence.Movies.Queries;
using Cinedex.Infrastructure.Authentication;
using Cinedex.Infrastructure.Persistence.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cinedex.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        return services
            .AddAuthenticationServices()
            .AddPersistenceServices();
    }
    
    private static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IMoviesQuery, MoviesQuery>();
    }
    
    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration(JwtOptions.SectionName);
        
        return services
            .AddScoped<ITokenProvider, JwtTokenProvider>();
    }
}