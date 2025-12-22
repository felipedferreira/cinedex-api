using Cinedex.Application.Abstractions.Movies.Queries;

namespace Cinedex.Application.Movies.Queries.GetMovies;

public class GetMoviesHandler(IMoviesQuery moviesQuery)
{
    public async Task<IEnumerable<MovieDetails>> HandleAsync(CancellationToken cancellationToken)
    {
        var movies = await moviesQuery.GetAllMoviesAsync(cancellationToken);
        return movies
            .Select(m => new MovieDetails
            {
                Id = m.Id,
                Title = m.Title,
                Year = m.Year,
            });
    }
}