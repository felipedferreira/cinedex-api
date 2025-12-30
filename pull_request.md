# Removes API Versioning and Implement Authentication

## Summary

This pull request removes API versioning from the codebase and introduces JWT-based authentication with comprehensive security features. The changes consolidate the API structure by moving endpoints from versioned directories to a simplified non-versioned architecture, while adding authentication, authorization, and CSRF protection capabilities.

## Changes

### 1. API Versioning Removal

**Endpoints Restructured:**
- **Movies Endpoints**: Consolidated `CreateMovie` and `GetMovies` endpoints from individual `v1/` directories into a unified `MoviesEndpoints.cs` file
  - `Features/Movies/CreateMovie/v1/CreateMovieEndpoint.cs` → `Features/Movies/MoviesEndpoints.cs`
  - `Features/Movies/GetMovies/v1/GetMoviesEndpoint.cs` → `Features/Movies/MoviesEndpoints.cs`
- **Request/Response Models**: Moved to parent feature directories without version folders
  - `CreateMovieRequest.cs` moved from `v1/` to `CreateMovie/`
  - `MovieResponse.cs` moved from `v1/` to `GetMovies/`

**Rationale**: Simplifies the codebase structure and removes unnecessary complexity from API versioning that was not being actively utilized.

### 2. Authentication Implementation

**New Components:**

#### Token Provider Interface and Implementation
- `ITokenProvider` interface in `Cinedex.Application.Abstractions.Authentication`
- `JwtTokenProvider` implementation in `Cinedex.Infrastructure.Authentication`
- `JwtOptions` configuration class with validation attributes

#### Authentication Endpoints
- `POST /login` - Authenticates users and returns JWT token
  - Issues JWT access token in response body
  - Sets HTTP-only refresh token cookie with secure configuration
  - Cookie settings: `HttpOnly`, `Secure`, `SameSite=Strict`, 7-day expiration
- `POST /refresh` - Refreshes authentication tokens
  - Validates CSRF token via antiforgery middleware
  - Reads refresh token from HTTP-only cookie

**Configuration:**
- JWT settings added to `appsettings.json` with configurable Issuer, Audience, and Secret
- ASP.NET Core authentication middleware configured with `JwtBearer` scheme
- Token validation parameters including issuer, audience, lifetime, and signing key validation

### 3. CSRF/XSRF Protection

**Antiforgery Implementation:**
- Custom antiforgery configuration with standardized header and cookie names
  - Header: `X-XSRF-TOKEN`
  - Cookie: `XSRF-TOKEN`
- Cookie configured as non-HttpOnly to allow JavaScript access for SPA integration
- `AntiforgeryConstants` class for centralized constant management
- New `GET /csrf` endpoint to obtain CSRF tokens
- Antiforgery middleware integration in request pipeline

**Security Features:**
- CSRF protection on token refresh endpoint
- Secure cookie policy enforcement (HTTPS only)
- Strict same-site policy on cookies

### 4. CORS Configuration Updates

**Changes:**
- Updated allowed origin from `http://localhost:3000` to `https://localhost:3000` (HTTPS)
- Added `.AllowCredentials()` to CORS policy to support cookie-based authentication
- Removed `.AllowAnyOrigin()` (incompatible with credentials)

### 5. Namespace Reorganization

**Infrastructure Layer:**
- Moved query implementations under `Persistence` namespace
  - `Cinedex.Infrastructure.Queries` → `Cinedex.Infrastructure.Persistence.Queries`
- Moved query interfaces and models under `Persistence` namespace
  - `Cinedex.Application.Abstractions.Movies.Queries` → `Cinedex.Application.Abstractions.Persistence.Movies.Queries`
- Updated `DependencyInjectionExtensions` to separate authentication and persistence service registration

**Rationale**: Better separation of concerns between persistence and other infrastructure responsibilities.

### 6. Test Project Renaming

**Changes:**
- Renamed integration test project: `Cinedex.Web.IntegrationTests` → `Cinedex.Integration.Web`
- Updated solution folder name: `tests` → `Tests` (capitalized)
- Maintained `RootNamespace` as `Cinedex.Web.IntegrationTests` for backward compatibility
- Updated namespaces in test files accordingly

### 7. Project Configuration

**Web Project:**
- Added `UserSecretsId` for local secret management
- Added package references:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.11)
  - `Microsoft.Extensions.Configuration.Abstractions` (9.0.11)

**Infrastructure Project:**
- New authentication-related package dependencies
- Empty `Persistence/` folder structure for future database implementations

## Breaking Changes

- API endpoints no longer include `/v1/` prefix
- CORS configuration now requires HTTPS for local development
- Authentication endpoints return different response formats

## Security Considerations

- JWT tokens stored in response body (client responsible for storage)
- Refresh tokens in HTTP-only cookies (protected from XSS)
- CSRF protection via antiforgery tokens
- Secure cookie policies enforced (HTTPS, SameSite=Strict)
- JWT secret should be configured via user secrets or environment variables (placeholder in appsettings.json)

## Testing

- Integration tests updated to reflect new project structure
- Existing `GetMoviesEndpoint` integration tests maintained and passing

## Migration Notes

Clients consuming this API will need to:
1. Update endpoint URLs to remove `/v1/` prefix
2. Use HTTPS for local development
3. Implement CSRF token handling for authenticated requests
4. Handle new authentication flow with JWT and refresh tokens

## Files Changed

- 20 files modified
- 10 files added
- 5 files deleted
- 5 files renamed

## Related Configuration

JWT configuration example (use user secrets or environment variables for production):
```json
{
  "Jwt": {
    "Issuer": "https://www.felipe-ferreira.codes/movie-svc",
    "Audience": "movie-svc",
    "Secret": "<secret-key-placeholder>"
  }
}
```
