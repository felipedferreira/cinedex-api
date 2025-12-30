using Cinedex.Application.Abstractions.Persistence.Movies.Queries;

namespace Cinedex.Application.Movies.GetMovies;

public class GetMoviesUseCase(IMoviesQuery moviesQuery)
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