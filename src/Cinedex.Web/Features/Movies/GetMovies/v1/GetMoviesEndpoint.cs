namespace Cinedex.Web.Features.Movies.GetMovies.v1;

internal static class GetMoviesEndpoint
{
    internal static IEndpointRouteBuilder MapGetMoviesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/movies", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Received request to get movies");

            // TODO: Replace with real data access. Returning a simple stubbed list to match patterns.
            var movies = new[]
            {
                new MovieDto(Guid.NewGuid(), "The Example Movie", 2024),
                new MovieDto(Guid.NewGuid(), "Another Example", 2023)
            };

            var response = new GetMoviesResponse(movies);

            return Results.Ok(response);
        })
        .WithName("GetMovies")
        .WithTags("Movies");

        return app;
    }
}

