using Cinedex.Application.Movies.GetMovies;
using Cinedex.Web.Features.Movies.CreateMovie;
using Cinedex.Web.Features.Movies.GetMovies;

namespace Cinedex.Web.Features.Movies;

internal static class MoviesEndpoints
{
    internal static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder app)
    {
        var moviesGroup = app.MapGroup("/movies").WithTags("Movies");
        
        moviesGroup.MapPost("/", (CreateMovieRequest request, ILogger<Program> logger) =>
        {
            logger.LogInformation("Received request to create a new movie: {MovieTitle}", request.Title);
            return Results.Created();
        })
        .WithName("CreateMovie")
        .WithSummary("Creates a new movie")
        .WithDescription("Adds a new movie to the catalog with the provided title, description, and release year.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        moviesGroup.MapGet("/", async (ILogger<Program> logger, GetMoviesUseCase useCase, CancellationToken ct) =>
        {
            logger.LogInformation("Received request to get movies");
            var movieDetails = await useCase.HandleAsync(ct);
            var movies = movieDetails.Select(movieDetail => new MovieResponse(movieDetail.Id, movieDetail.Title, movieDetail.Year));
            return Results.Ok(movies);
        })
        .WithName("GetMovies")
        .WithSummary("Retrieves all movies")
        .WithDescription("Returns a list of all movies in the catalog, including their ID, title, and release year.")
        .Produces<IEnumerable<MovieResponse>>(StatusCodes.Status200OK);

        return app;
    }
}
