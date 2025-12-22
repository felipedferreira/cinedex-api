namespace Cinedex.Domain;

public class Movie
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public int Year { get; set; }
}