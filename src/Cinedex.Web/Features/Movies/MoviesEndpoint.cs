using Cinedex.Application.Movies.Queries.GetMovies;
using Cinedex.Web.Features.Movies.Models;

namespace Cinedex.Web.Features.Movies;

internal static class MoviesEndpoint
{
    internal static IEndpointRouteBuilder MapMoviesEndpoint(this IEndpointRouteBuilder app)
    {
        var moviesGroup = app.MapGroup("/movies")
            .WithTags("Movies");

        moviesGroup.MapGet("", GetMovies)
            .WithName("GetMovies");

        moviesGroup.MapPost("", CreateMovie)
            .WithName("CreateMovie");

        return app;
    }

    private static async Task<IResult> GetMovies(
        ILogger<Program> logger,
        GetMoviesHandler handler,
        CancellationToken ct)
    {
        logger.LogInformation("Received request to get movies");
        var movieDetails = await handler.HandleAsync(ct);
        var movies = movieDetails.Select(movieDetail =>
            new MovieResponse(movieDetail.Id, movieDetail.Title, movieDetail.Year));
        return Results.Ok(movies);
    }

    private static IResult CreateMovie(
        CreateMovieRequest request,
        ILogger<Program> logger)
    {
        logger.LogInformation("Received request to create a new movie: {MovieTitle}", request.Title);
        return Results.Created();
    }
}
