using Cinedex.Application.Abstractions.Movies.Queries;
using Cinedex.Infrastructure.Queries;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        return services.AddScoped<IMoviesQuery, MoviesQuery>();
    }
}