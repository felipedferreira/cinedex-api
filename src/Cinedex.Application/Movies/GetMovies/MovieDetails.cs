namespace Cinedex.Application.Movies.GetMovies;

public class MovieDetails
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }
}