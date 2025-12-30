using Cinedex.Web.Constants;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cinedex.Web.IntegrationTests.TestHelpers;

public class CinedexWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Reconfigure antiforgery for test environment to allow HTTP requests
            services.AddAntiforgery(options =>
            {
                options.HeaderName = AntiforgeryConstants.XsrfHeader;
                options.Cookie.Name = AntiforgeryConstants.XsrfCookie;
                options.Cookie.HttpOnly = false;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP in tests
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = "/";
            });
        });
    }
}