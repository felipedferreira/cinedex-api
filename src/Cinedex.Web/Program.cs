using System.Text;
using Cinedex.Application;
using Cinedex.Web.Features.Authentication.Login.v1;
using Microsoft.AspNetCore.HttpOverrides;
using Cinedex.Web.Features.Movies.CreateMovie.v1;
using Cinedex.Web.Features.Movies.GetMovies.v1;
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
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        builder.Services.AddApplicationServices();
        builder.Services.AddEndpointsApiExplorer();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourdomain.com",
                    ValidAudience = "yourdomain.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_super_secret_key")),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.Logger.LogInformation("Application is running in Development mode.");
            app.MapOpenApi();
            // --- 👇 Add Scalar UI (the modern Swagger alternative) ---
            app.MapScalarApiReference(options =>
            {
                options.Title = "🎬 MovieBuff API";
                options.Theme = ScalarTheme.BluePlanet;
                options.DarkMode = true;
                options.WithBaseServerUrl("/movie-svc");
            });
        }
        
        // logs requests
        app.UseSerilogRequestLogging();
        
        // Adds middleware for redirecting HTTP Requests to HTTPS. 
        app.UseHttpsRedirection();
        
        // Sets the path base to "movie-svc" so that all endpoints are prefixed with this path.
        app.UsePathBase("/movie-svc");
        if (corsEnabled)
        {
            app.UseCors("DevCors");
        }

        // Maps endpoints
        app.MapCreateMovieEndpoint();
        app.MapGetMoviesEndpoint();
        app.MapLoginEndpoint();

        app.Logger.LogInformation("Application has started.");
        
        await app.RunAsync();
    }
}