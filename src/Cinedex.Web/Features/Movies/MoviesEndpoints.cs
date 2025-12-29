using Cinedex.Application.Movies.Queries.GetMovies;
using Cinedex.Web.Features.Movies.CreateMovie;
using Cinedex.Web.Features.Movies.GetMovies;

namespace Cinedex.Web.Features.Movies;

internal static class MoviesEndpoints
{
    internal static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/movies", (CreateMovieRequest request, ILogger<Program> logger) =>
        {
            logger.LogInformation("Received request to create a new movie: {MovieTitle}", request.Title);
            return Results.Created();
        })
        .WithName("CreateMovie")
        .WithTags("Movies");

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
