using Cinedex.Application.Authentication.Login;
using Cinedex.Application.Authentication.Refresh;
using Cinedex.Application.Movies.GetMovies;
using Cinedex.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinedex.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<GetMoviesUseCase>()
            .AddScoped<LoginUseCase>()
            .AddScoped<RefreshUseCase>()
            .AddInfrastructureServices();
    }
}