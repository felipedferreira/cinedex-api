using Cinedex.Application.Abstractions.Persistence.Movies.Queries;

namespace Cinedex.Infrastructure.Persistence.Queries;

public class MoviesQuery : IMoviesQuery
{
    public async Task<IEnumerable<MovieReadModel>> GetAllMoviesAsync(CancellationToken cancellationToken)
    {
        // Temporary hard-coded data for demonstration purposes only.
        return [
            new MovieReadModel
            {
                Id = Guid.NewGuid(),
                Title = "The Flintstones",
                Year = 1994,
            },
            new MovieReadModel
            {
                Id = Guid.NewGuid(),
                Title = "Titanic",
                Year = 1997,
            },
        ];
    }
}