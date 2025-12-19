namespace Cinedex.Web.Features.Movies.GetMovies.v1;

internal sealed record GetMoviesResponse(IEnumerable<MovieDto> Movies);