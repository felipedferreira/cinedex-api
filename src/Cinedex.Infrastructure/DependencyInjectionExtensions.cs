using Cinedex.Application.Abstractions.Movies.Queries;
using Cinedex.Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cinedex.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        return services.AddScoped<IMoviesQuery, MoviesQuery>();
    }
}