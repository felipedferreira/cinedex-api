namespace Cinedex.Application.Abstractions.Persistence.Movies.Queries;

public class MovieReadModel
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public int Year { get; init; }
}