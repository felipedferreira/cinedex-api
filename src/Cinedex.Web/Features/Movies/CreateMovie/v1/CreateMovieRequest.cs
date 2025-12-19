using System.ComponentModel.DataAnnotations;

namespace Cinedex.Web.Features.Movies.CreateMovie.v1;

public class CreateMovieRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(250)]
    public string Title { get; set; }
    
    [Required]
    [Range(1_950, 3_000, ErrorMessage = "Year of release must be after 1950 but before 3000.")]
    public int YearOfRelease { get; set; }

    [Required(ErrorMessage = "At least one genre must be specified.")]
    [MinLength(1, ErrorMessage = "At least one genre must be specified.")]
    public IEnumerable<string> Genres { get; set; } = [];
}