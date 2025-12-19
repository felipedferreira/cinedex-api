namespace Cinedex.Web.Features.Movies.CreateMovie.v1;

internal static class CreateMovieEndpoint
{
    internal static IEndpointRouteBuilder MapCreateMovieEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/movies", (CreateMovieRequest request, ILogger<Program> logger) =>
        {
            logger.LogInformation("Received request to create a new movie: {MovieTitle}", request.Title);
            return Results.Created();
        })
        .WithName("CreateMovie")
        .WithTags("Movies");

        return app;
    }
}