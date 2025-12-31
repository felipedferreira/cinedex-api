using System.Net;
using System.Net.Http.Json;
using Cinedex.Web.Constants;
using Cinedex.Web.Features.Authentication.Login;
using Cinedex.Web.IntegrationTests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cinedex.Web.IntegrationTests.Features.Authentication;

public class AuthenticationEndpointsIntegrationTests : IClassFixture<CinedexWebApplicationFactory>
{
    private readonly CinedexWebApplicationFactory _factory;

    public AuthenticationEndpointsIntegrationTests(CinedexWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RefreshToken_ReturnsNewAccessToken_WhenValidCsrfTokenAndRefreshTokenProvided()
    {
        // Arrange - Create HTTP client that doesn't handle cookies automatically
        using var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false // We'll manage cookies manually
        });

        // Step 1: Login to get initial access token and refresh token cookie
        var loginRequest = new LoginRequest("test@example.com", "Password123!");
        var loginResponse = await httpClient.PostAsJsonAsync("/movie-svc/authentication/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var accessToken = await loginResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(accessToken), "Access token should not be empty");

        // Extract refresh token from Set-Cookie header
        var refreshTokenCookie = ExtractCookie(loginResponse, AuthenticationConstants.RefreshTokenCookie);
        Assert.NotNull(refreshTokenCookie);

        // Step 2: Get CSRF token
        var csrfResponse = await httpClient.PostAsync("/movie-svc/security/csrf", null);
        Assert.Equal(HttpStatusCode.OK, csrfResponse.StatusCode);

        var csrfToken = await csrfResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(csrfToken), "CSRF token should not be empty");

        // Extract XSRF cookie from Set-Cookie header
        var xsrfCookie = ExtractCookie(csrfResponse, AntiforgeryConstants.XsrfCookie);
        Assert.NotNull(xsrfCookie);

        // Step 3: Refresh the token with CSRF token in header and cookies
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/movie-svc/authentication/refresh");
        refreshRequest.Headers.Add(AntiforgeryConstants.XsrfHeader, csrfToken);
        refreshRequest.Headers.Add("Cookie", $"{AuthenticationConstants.RefreshTokenCookie}={refreshTokenCookie}; {AntiforgeryConstants.XsrfCookie}={xsrfCookie}");

        var refreshResponse = await httpClient.SendAsync(refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var newAccessToken = await refreshResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(newAccessToken), "New access token should not be empty");

        // Verify a new refresh token cookie was set (token rotation)
        var newRefreshTokenCookie = ExtractCookie(refreshResponse, AuthenticationConstants.RefreshTokenCookie);
        Assert.NotNull(newRefreshTokenCookie);
    }

    [Fact(Skip = "Temporarily skipped")]
    public async Task RefreshToken_Returns403Forbidden_WhenCsrfTokenIsMissing()
    {
        // Arrange - Create HTTP client with cookie support
        var cookieContainer = new CookieContainer();
        using var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        using var clientWithCookies = new HttpClient(handler) { BaseAddress = httpClient.BaseAddress };

        // Step 1: Login to get refresh token cookie
        var loginRequest = new LoginRequest("test@example.com", "Password123!");
        var loginResponse = await clientWithCookies.PostAsJsonAsync("/movie-svc/authentication/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Step 2: Get CSRF token (but don't use it)
        var csrfResponse = await httpClient.PostAsync("/movie-svc/security/csrf", null);
        Assert.Equal(HttpStatusCode.OK, csrfResponse.StatusCode);

        // Step 3: Try to refresh without CSRF header
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/movie-svc/authentication/refresh");
        // Intentionally NOT adding the X-XSRF-TOKEN header

        var refreshResponse = await clientWithCookies.SendAsync(refreshRequest);

        // Assert - Should return 403 Forbidden due to missing CSRF token
        Assert.Equal(HttpStatusCode.Forbidden, refreshResponse.StatusCode);
    }

    [Fact(Skip = "Temporarily skipped")]
    public async Task RefreshToken_Returns401Unauthorized_WhenRefreshTokenCookieIsMissing()
    {
        // Arrange - Create HTTP client with cookie support
        var cookieContainer = new CookieContainer();
        using var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        using var clientWithCookies = new HttpClient(handler) { BaseAddress = httpClient.BaseAddress };

        // Step 1: Get CSRF token
        var csrfResponse = await clientWithCookies.PostAsync("/movie-svc/security/csrf", null);
        Assert.Equal(HttpStatusCode.OK, csrfResponse.StatusCode);

        var csrfToken = await csrfResponse.Content.ReadAsStringAsync();

        // Step 2: Try to refresh without having logged in (no refresh token cookie)
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/movie-svc/authentication/refresh");
        refreshRequest.Headers.Add(AntiforgeryConstants.XsrfHeader, csrfToken);

        var refreshResponse = await clientWithCookies.SendAsync(refreshRequest);

        // Assert - Should return 401 Unauthorized due to missing refresh token
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    private static string? ExtractCookie(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return null;
        }

        foreach (var setCookie in setCookieHeaders)
        {
            var cookieParts = setCookie.Split(';')[0].Split('=');
            if (cookieParts.Length == 2 && cookieParts[0].Trim() == cookieName)
            {
                return cookieParts[1].Trim();
            }
        }

        return null;
    }
}
