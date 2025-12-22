namespace Cinedex.Application.Abstractions.Persistence.Movies.Queries;

public interface IMoviesQuery
{
    Task<IEnumerable<MovieReadModel>> GetAllMoviesAsync(CancellationToken cancellationToken);
}