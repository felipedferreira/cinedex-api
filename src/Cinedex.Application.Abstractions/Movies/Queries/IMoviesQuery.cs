namespace Cinedex.Application.Abstractions.Movies.Queries;

public interface IMoviesQuery
{
    Task<IEnumerable<MovieReadModel>> GetAllMoviesAsync(CancellationToken cancellationToken);
}