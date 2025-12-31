using System.Net.Mime;
using System.Text;
using Cinedex.Application;
using Cinedex.Web.Constants;
using Cinedex.Web.Extensions;
using Cinedex.Web.Features.Authentication;
using Cinedex.Web.Middleware.ExceptionHandlers;
using Microsoft.AspNetCore.HttpOverrides;
using Cinedex.Web.Features.Movies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Cinedex.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog programmatically (reads config and ensures console sink with theme)
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate);
        });
        
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryConstants.XsrfHeader;
            options.Cookie.Name = AntiforgeryConstants.XsrfCookie;
            options.Cookie.HttpOnly = false;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
        });
        
        var corsEnabled = builder.Configuration.GetValue<bool>("CORS:Enabled");
        if (corsEnabled)
        {
            var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DevCors", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins ?? [])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        builder.Services.AddApplicationServices();
        builder.Services.AddCustomProblemDetails();
        builder.Services.AddEndpointsApiExplorer();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Cinedex API";
                document.Info.Version = "1.0";
                return Task.CompletedTask;
            });
        });
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        builder.Services.AddAuthorization();
        // Register exception handlers in order (specific → general)
        builder.Services.AddExceptionHandler<AuthenticationExceptionHandler>();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var app = builder.Build();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.Logger.LogInformation("Application is running in Development mode.");
            app.MapOpenApi();
            // --- 👇 Add Scalar UI (the modern Swagger alternative) ---
            app.MapScalarApiReference(options =>
            {
                options.DarkMode = true;
                options.DefaultHttpClient = new(ScalarTarget.Shell, ScalarClient.Curl);
                options.EnabledClients = [ScalarClient.HttpClient, ScalarClient.Curl, ScalarClient.Fetch, ScalarClient.Axios];
                options.EnabledTargets = [ScalarTarget.Shell, ScalarTarget.CSharp, ScalarTarget.JavaScript];
                options
                    .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl)
                    .WithClassicLayout()
                    .WithBaseServerUrl(PathConstants.ApiBasePath)
                    .WithTheme(ScalarTheme.BluePlanet)
                    .WithTitle("🎬 MovieBuff API");
            });
        }
        
        // logs requests
        app.UseSerilogRequestLogging();

        // Global exception handling
        app.UseExceptionHandler();

        // Handle status code responses (401, 403, 404, etc.) that have no body
        app.UseStatusCodePages();

        // Adds middleware for redirecting HTTP Requests to HTTPS.
        app.UseHttpsRedirection();

        // Middleware to redirect requests without API base path prefix
        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments(PathConstants.ApiBasePath))
            {
                context.Response.Redirect($"{PathConstants.ApiBasePath}{context.Request.Path}{context.Request.QueryString}", permanent: true);
                return;
            }
            await next();
        });

        // Sets the path base to "movie-svc" so that all endpoints are prefixed with this path.
        app.UsePathBase(PathConstants.ApiBasePath);
        if (corsEnabled)
        {
            app.UseCors("DevCors");
        }
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        
        // Maps endpoints
        app.MapMoviesEndpoints();
        app.MapAuthenticationEndpoints();
        app.MapGroup("/security")
            .WithTags("Security")
            .MapPost("/csrf", (IAntiforgery antiforgery, HttpContext ctx) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(ctx);
                return Results.Content(tokens.RequestToken, MediaTypeNames.Text.Plain);
            })
            .WithSummary("Creates CSRF token")
            .WithDescription("Creates and returns a new CSRF token for client-side form submissions. The token should be included in the X-XSRF-TOKEN header for state-changing requests.")
            .Produces<string>(StatusCodes.Status200OK, MediaTypeNames.Text.Plain);

        app.Logger.LogInformation("Application has started.");
        
        await app.RunAsync();
    }
}