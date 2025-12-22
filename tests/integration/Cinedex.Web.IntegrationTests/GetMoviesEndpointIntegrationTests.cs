using System.Net.Http.Json;
using Cinedex.Web.Features.Movies.GetMovies.v1;
using Cinedex.Web.IntegrationTests.TestHelpers;

namespace Cinedex.Web.IntegrationTests;

public class GetMoviesEndpointIntegrationTests : IClassFixture<CinedexWebApplicationFactory>
{
    private readonly CinedexWebApplicationFactory _factory;
    public GetMoviesEndpointIntegrationTests(CinedexWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task GetMoviesAsync_ReturnsMovies_WhenMoviesExist()
    {
        // Arrange
        using var httpClient = _factory.CreateDefaultClient();
        
        // Act
        using var httpResponse = await httpClient.GetAsync("/movie-svc/movies");
        var sampleMovies = await httpResponse.Content.ReadAsStringAsync();
        var movies = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<MovieResponse>>();
        
        // Assert
        Assert.NotNull(movies);
    }
}