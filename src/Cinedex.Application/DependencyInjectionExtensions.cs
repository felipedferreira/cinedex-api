using Cinedex.Application.Movies.Queries.GetMovies;
using Cinedex.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinedex.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<GetMoviesHandler>()
            .AddInfrastructureServices();
    }
}