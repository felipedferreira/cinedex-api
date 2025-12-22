using Cinedex.Application.Movies.Queries.GetMovies;

namespace Cinedex.Web.Features.Movies.GetMovies.v1;

internal static class GetMoviesEndpoint
{
    internal static IEndpointRouteBuilder MapGetMoviesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/movies", async (ILogger<Program> logger, GetMoviesHandler handler, CancellationToken ct) =>
        {
            logger.LogInformation("Received request to get movies");
            var movieDetails = await handler.HandleAsync(ct);
            var movies = movieDetails.Select(movieDetail => new MovieResponse(movieDetail.Id, movieDetail.Title, movieDetail.Year));
            return Results.Ok(movies);
        })
        .WithName("GetMovies")
        .WithTags("Movies");

        return app;
    }
}

