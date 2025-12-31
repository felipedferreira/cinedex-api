# Middleware Pipeline Documentation

This document provides a comprehensive explanation of the middleware pipeline in the Cinedex API, including the order of execution, responsibilities, and how error handling works.

## Table of Contents
- [What is Middleware?](#what-is-middleware)
- [Middleware Pipeline Order](#middleware-pipeline-order)
- [Detailed Middleware Breakdown](#detailed-middleware-breakdown)
- [Error Handling Strategy](#error-handling-strategy)
- [Request Flow Example](#request-flow-example)

## What is Middleware?

Middleware components in ASP.NET Core form a pipeline that handles HTTP requests and responses. Each middleware:
- Can process the request before passing it to the next middleware
- Can process the response after the next middleware has executed
- Can short-circuit the pipeline by not calling the next middleware

Think of it as layers of an onion - the request goes through each layer inward, and the response comes back through each layer outward.

```
Request  →  [Middleware 1]  →  [Middleware 2]  →  [Endpoint]
Response ←  [Middleware 1]  ←  [Middleware 2]  ←  [Endpoint]
```

## Middleware Pipeline Order

**CRITICAL:** Order matters! Middleware executes in the order it's registered.

```
1. UseForwardedHeaders
2. MapOpenApi (Development only)
3. MapScalarApiReference (Development only)
4. UseSerilogRequestLogging
5. UseExceptionHandler          ← Exception handling
6. UseStatusCodePages           ← Status code handling
7. UseHttpsRedirection
8. Custom path redirect middleware
9. UsePathBase
10. UseCors (if enabled)
11. UseAuthentication
12. UseAuthorization
13. UseAntiforgery
14. Endpoints (MapMoviesEndpoints, MapAuthenticationEndpoints, etc.)
```

**File location:** `src/Cinedex.Web/Program.cs`

## Detailed Middleware Breakdown

### 1. UseForwardedHeaders
**Purpose:** Handles `X-Forwarded-*` headers from proxies/load balancers

**What it does:**
- Updates request information (IP address, protocol) based on proxy headers
- Essential when running behind reverse proxies (nginx, Azure App Gateway, etc.)
- Ensures your app sees the real client IP, not the proxy's IP

**Configuration:**
```csharp
ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
```
- `XForwardedFor` - Original client IP address
- `XForwardedProto` - Original protocol (HTTP/HTTPS)

**Why it's first:**
- Must run before any middleware that needs accurate request information
- Logging, authentication, and authorization depend on correct request data

---

### 2. MapOpenApi (Development Only)
**Purpose:** Exposes OpenAPI/Swagger JSON document

**What it does:**
- Generates OpenAPI specification endpoint at `/openapi/v1.json`
- Describes all API endpoints, request/response schemas, authentication
- Only available in Development environment

---

### 3. MapScalarApiReference (Development Only)
**Purpose:** Interactive API documentation UI (modern Swagger alternative)

**What it does:**
- Provides beautiful, interactive API documentation at `/scalar/v1`
- Allows testing endpoints directly from the browser
- Configured with dark mode, code examples (curl, C#, JS), and custom branding

**Configuration highlights:**
- Theme: Blue Planet
- Title: "🎬 MovieBuff API"
- Supports curl, HttpClient, Fetch, Axios examples

---

### 4. UseSerilogRequestLogging
**Purpose:** Logs all HTTP requests and responses

**What it does:**
- Records request method, path, status code, duration
- Structured logging (JSON format) for easy querying
- Configurable log levels based on status codes

**Why positioned here:**
- After proxy headers (to log correct client IP)
- Before exception handling (to log failed requests)
- Logs the entire request/response cycle

**Example log output:**
```
HTTP GET /movie-svc/movies responded 200 in 45.2ms
```

---

### 5. UseExceptionHandler ⭐
**Purpose:** Global exception handling for thrown exceptions

**What it does:**
- Catches **any unhandled exception** thrown during request processing
- Routes to registered `IExceptionHandler` implementations
- Prevents exceptions from crashing the application

**Exception Handlers (executed in order):**

#### a. AuthenticationExceptionHandler
**File:** `src/Cinedex.Web/Middleware/ExceptionHandlers/AuthenticationExceptionHandler.cs`

**Catches:**
- `InvalidRefreshTokenException`
- `RefreshTokenExpiredException`
- `RefreshTokenNotFoundException`
- `RefreshTokenRevokedException`

**Response:** 401 Unauthorized
```json
{
  "type": "https://httpstatuses.com/401",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid refresh token",
  "instance": "/movie-svc/authentication/refresh",
  "traceId": "00-abc123..."
}
```

**Security note:** Returns generic message to prevent user enumeration attacks. Specific exception type logged server-side only.

#### b. ValidationExceptionHandler
**File:** `src/Cinedex.Web/Middleware/ExceptionHandlers/ValidationExceptionHandler.cs`

**Catches:** `System.ComponentModel.DataAnnotations.ValidationException`

**Response:** 400 Bad Request
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "validation": ["Error message"]
  },
  "instance": "/movie-svc/movies",
  "traceId": "00-abc123..."
}
```

**Note:** Currently a placeholder. Will be enhanced when FluentValidation is added.

#### c. GlobalExceptionHandler
**File:** `src/Cinedex.Web/Middleware/ExceptionHandlers/GlobalExceptionHandler.cs`

**Catches:** Everything else (NullReferenceException, SqlException, etc.)

**Response:** 500 Internal Server Error

**Development environment:**
```json
{
  "type": "https://httpstatuses.com/500",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Object reference not set to an instance of an object.",
  "instance": "/movie-svc/movies",
  "traceId": "00-abc123...",
  "stackTrace": "at Cinedex.Application.Movies...",
  "exceptionType": "NullReferenceException"
}
```

**Production environment:**
```json
{
  "type": "https://httpstatuses.com/500",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred while processing your request.",
  "instance": "/movie-svc/movies",
  "traceId": "00-abc123..."
}
```

**Why it's here:**
- After request logging (exceptions get logged)
- Before all other middleware (catches errors from entire pipeline)
- First line of defense against unhandled exceptions

---

### 6. UseStatusCodePages ⭐
**Purpose:** Handles HTTP status codes that have no response body

**What it does:**
- Intercepts responses with 4xx/5xx status codes
- Only activates if no response body has been written
- Automatically generates ProblemDetails responses

**Common scenarios:**
- Authorization middleware returns 401/403 without body
- Antiforgery validation fails
- Custom middleware sets status code without writing response

**Example (unauthorized access):**
1. Request to protected endpoint without authentication
2. `UseAuthorization` sets `Response.StatusCode = 401`
3. No exception thrown, no body written
4. `UseStatusCodePages` intercepts 401
5. Writes ProblemDetails response

**Why it's here:**
- After exception handler (only handles non-exception status codes)
- Before authentication/authorization (catches their status codes)

---

### 7. UseHttpsRedirection
**Purpose:** Redirects HTTP requests to HTTPS

**What it does:**
- Returns 307 Temporary Redirect for HTTP requests
- Rewrites URL to use `https://` scheme
- Ensures all traffic uses encrypted connections

**Why it's here:**
- After error handling (errors during redirect are handled)
- Before authentication (credentials should only travel over HTTPS)

---

### 8. Custom Path Redirect Middleware
**Purpose:** Redirects requests without API base path to correct path

**What it does:**
- Checks if request path starts with `/movie-svc`
- If not, redirects to `/movie-svc{original-path}`
- Returns 301 Permanent Redirect

**Example:**
- Request: `GET /movies` → Redirects to `GET /movie-svc/movies`
- Request: `GET /movie-svc/movies` → Continues normally

**Why it's here:**
- After HTTPS redirect (correct protocol first)
- Before path base (handles missing prefix)

---

### 9. UsePathBase
**Purpose:** Sets the base path for all requests

**What it does:**
- Strips `/movie-svc` prefix from request path for routing
- Adds `/movie-svc` prefix to all generated URLs
- Allows hosting multiple APIs under different paths

**Example:**
- Request: `GET /movie-svc/movies`
- Routing sees: `GET /movies`
- Response links include: `/movie-svc/...`

**Configuration:**
```csharp
PathConstants.ApiBasePath = "/movie-svc"
```

**Why it's here:**
- After path redirect (ensures prefix exists)
- Before routing and endpoints (simplifies route definitions)

---

### 10. UseCors (Conditional)
**Purpose:** Handles Cross-Origin Resource Sharing (CORS)

**What it does:**
- Allows cross-origin requests from configured origins
- Sets CORS headers (`Access-Control-Allow-Origin`, etc.)
- Handles preflight OPTIONS requests

**Configuration (when enabled):**
```csharp
policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
```

**Configuration source:** `appsettings.json`
```json
{
  "CORS": {
    "Enabled": true,
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

**Why it's here:**
- After path base (correct path for CORS checks)
- Before authentication (CORS must be checked first)

---

### 11. UseAuthentication
**Purpose:** Authenticates incoming requests

**What it does:**
- Validates JWT tokens from `Authorization` header
- Populates `HttpContext.User` with claims from token
- Does NOT reject requests - just identifies the user

**JWT Configuration:**
```csharp
ValidateIssuer = true
ValidateAudience = true
ValidateLifetime = true
ValidateIssuerSigningKey = true
ClockSkew = 30 seconds
```

**Why it's here:**
- After CORS (CORS preflight requests don't need auth)
- Before authorization (authorization needs user identity)

**Important:** Authentication identifies WHO you are. Authorization determines WHAT you can do.

---

### 12. UseAuthorization
**Purpose:** Authorizes requests based on policies/requirements

**What it does:**
- Checks if authenticated user has permission for the endpoint
- Evaluates `[Authorize]` attributes and `.RequireAuthorization()`
- Sets status code to 401/403 if unauthorized (no body)

**Responses:**
- 401 Unauthorized - Not authenticated (no valid token)
- 403 Forbidden - Authenticated but not authorized (insufficient permissions)

**Why it's here:**
- After authentication (needs user identity)
- Before endpoints (must authorize before executing logic)

**Note:** 401/403 responses have no body, so `UseStatusCodePages` handles formatting

---

### 13. UseAntiforgery
**Purpose:** Protects against Cross-Site Request Forgery (CSRF) attacks

**What it does:**
- Validates CSRF tokens on state-changing requests (POST, PUT, DELETE)
- Validates `X-XSRF-TOKEN` header matches cookie
- Throws `AntiforgeryValidationException` if validation fails

**Configuration:**
```csharp
HeaderName = "X-XSRF-TOKEN"
Cookie.Name = "XSRF-TOKEN"
Cookie.HttpOnly = false  // JS needs to read it
Cookie.SecurePolicy = Always
Cookie.SameSite = Strict
```

**Token endpoint:** `GET /movie-svc/csrf` returns token

**Why it's here:**
- After authentication/authorization (CSRF for authenticated users)
- Before endpoints (validate before executing logic)

---

### 14. Endpoints
**Purpose:** Execute application logic and return responses

**Mapped endpoints:**
- `MapMoviesEndpoints()` - Movie CRUD operations
- `MapAuthenticationEndpoints()` - Login, refresh token
- `MapGet("/csrf", ...)` - CSRF token generation

**Endpoint example:**
```csharp
app.MapGet("/movies", async (IGetMoviesUseCase useCase) =>
{
    var movies = await useCase.ExecuteAsync();
    return Results.Ok(movies);
})
.RequireAuthorization()
.WithSummary("Get all movies");
```

**Why it's last:**
- All cross-cutting concerns handled by previous middleware
- Business logic executes in a safe, controlled environment
- Errors caught by exception handlers above

---

## Error Handling Strategy

The API uses a layered error handling approach with RFC 7807 ProblemDetails format:

### Layer 1: Exception Handlers (UseExceptionHandler)

**Handles:** Thrown exceptions

**Execution order:**
1. `AuthenticationExceptionHandler` - Try to handle, return `false` if not `AuthenticationException`
2. `ValidationExceptionHandler` - Try to handle, return `false` if not `ValidationException`
3. `GlobalExceptionHandler` - Catch everything else, always returns `true`

**Benefits:**
- Specific handlers for domain exceptions
- Environment-aware error details
- Structured logging with context
- Consistent ProblemDetails responses

### Layer 2: Status Code Pages (UseStatusCodePages)

**Handles:** Status codes without response bodies

**Common sources:**
- Authorization middleware (401, 403)
- Antiforgery middleware (400)
- Custom middleware setting status codes
- `return StatusCode(404)` without body

**Benefits:**
- Consistent formatting for all error responses
- No need for endpoints to manually format errors
- Automatic ProblemDetails serialization

### Configuration

Both layers integrate with:
```csharp
builder.Services.AddCustomProblemDetails();
```

This configures:
- Trace ID for request correlation
- Instance path from request
- RFC 7807 compliant type URIs
- Custom extensions (traceId)

**File:** `src/Cinedex.Web/Extensions/ProblemDetailsExtensions.cs`

---

## Request Flow Example

Let's trace a request through the entire pipeline:

### Scenario: Authenticated user requests movies

**Request:**
```http
GET /movie-svc/movies HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGc...
X-Forwarded-For: 203.0.113.42
X-Forwarded-Proto: https
```

**Flow:**

1. **UseForwardedHeaders**
   - Updates `HttpContext.Connection.RemoteIpAddress` to `203.0.113.42`
   - Updates `HttpContext.Request.Scheme` to `https`

2. **UseSerilogRequestLogging**
   - Starts timer
   - Records incoming request details

3. **UseExceptionHandler**
   - Wraps everything below in try/catch
   - Waits for response

4. **UseStatusCodePages**
   - Passes through (no action on request)

5. **UseHttpsRedirection**
   - Request already HTTPS, continues

6. **Custom path redirect**
   - Path starts with `/movie-svc`, continues

7. **UsePathBase**
   - Strips `/movie-svc` from path
   - Routing sees: `GET /movies`

8. **UseCors**
   - No CORS headers (same origin), continues

9. **UseAuthentication**
   - Validates JWT from `Authorization` header
   - Populates `HttpContext.User` with claims
   - Sets `User.Identity.IsAuthenticated = true`

10. **UseAuthorization**
    - Endpoint requires authorization
    - User is authenticated, authorized
    - Continues

11. **UseAntiforgery**
    - GET request, no validation needed
    - Continues

12. **Endpoint executes**
    - `GetMoviesUseCase.ExecuteAsync()` called
    - Returns list of movies
    - `Results.Ok(movies)` sets status 200, writes JSON

**Response bubbles back up:**

13. **UseStatusCodePages**
    - Status 200, has body
    - No action

14. **UseExceptionHandler**
    - No exception thrown
    - No action

15. **UseSerilogRequestLogging**
    - Stops timer
    - Logs: `HTTP GET /movie-svc/movies responded 200 in 42.3ms`

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json
[
  { "id": 1, "title": "The Matrix", ... }
]
```

---

### Scenario: Refresh token expired (Exception)

**Request:**
```http
POST /movie-svc/authentication/refresh HTTP/1.1
Content-Type: application/json

{
  "refreshToken": "expired_token_xyz"
}
```

**Flow:**

Steps 1-12 execute normally, then:

**Endpoint executes:**
```csharp
var result = await refreshUseCase.ExecuteAsync(request.RefreshToken);
// Inside RefreshUseCase:
throw new RefreshTokenExpiredException();
```

**Exception bubbles up:**

13-14. **UseAntiforgery, UseAuthorization, UseAuthentication**
    - Exception passes through (not handled)

15. **UseCors, UsePathBase, etc.**
    - Exception continues bubbling

16. **UseStatusCodePages**
    - Exception passes through (doesn't handle exceptions)

17. **UseExceptionHandler** ⭐
    - Catches `RefreshTokenExpiredException`
    - Routes to exception handlers:
      - `AuthenticationExceptionHandler` checks if `AuthenticationException`
      - It is! Handler executes:
        - Logs: `Warning: Authentication failed: RefreshTokenExpiredException - Token expired`
        - Sets status 401
        - Writes ProblemDetails JSON
        - Returns `true` (handled)
    - Pipeline stops (exception handled)

18. **UseSerilogRequestLogging**
    - Logs: `HTTP POST /movie-svc/authentication/refresh responded 401 in 15.2ms`

**Response:**
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/problem+json

{
  "type": "https://httpstatuses.com/401",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid refresh token",
  "instance": "/movie-svc/authentication/refresh",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

---

### Scenario: Unauthorized access (Status Code)

**Request:**
```http
GET /movie-svc/movies HTTP/1.1
Host: api.example.com
(No Authorization header)
```

**Flow:**

Steps 1-9 execute:

10. **UseAuthentication**
    - No `Authorization` header
    - `HttpContext.User.Identity.IsAuthenticated = false`

11. **UseAuthorization** ⭐
    - Endpoint requires authorization
    - User not authenticated
    - Sets `Response.StatusCode = 401`
    - Does NOT write response body
    - Continues (doesn't throw exception)

12. **Endpoint NOT executed**
    - Authorization short-circuited the pipeline

**Response bubbles back up:**

13. **UseStatusCodePages** ⭐
    - Sees status 401
    - Sees no response body written
    - Activates ProblemDetails generator
    - Writes ProblemDetails JSON

14. **UseExceptionHandler**
    - No exception, no action

15. **UseSerilogRequestLogging**
    - Logs: `HTTP GET /movie-svc/movies responded 401 in 8.1ms`

**Response:**
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/problem+json

{
  "type": "https://httpstatuses.com/401",
  "title": "Unauthorized",
  "status": 401,
  "instance": "/movie-svc/movies",
  "traceId": "00-7c0c5e1a8b7f4a6d9f2e3b1c0d8a9e7f-00"
}
```

---

## Summary

The middleware pipeline is carefully ordered to:

1. **Establish request context** (forwarded headers, logging)
2. **Handle errors comprehensively** (exceptions and status codes)
3. **Secure the connection** (HTTPS redirect)
4. **Route correctly** (path base)
5. **Enable cross-origin access** (CORS)
6. **Authenticate and authorize** (JWT validation, policies)
7. **Protect against attacks** (CSRF)
8. **Execute business logic** (endpoints)

This architecture ensures:
- ✅ All errors return consistent ProblemDetails format
- ✅ Requests are logged with proper context
- ✅ Security checks happen in the right order
- ✅ Sensitive information never leaks to clients
- ✅ Development experience includes detailed errors
- ✅ Production stays secure with generic messages

**Remember:** Middleware order is critical. Changing the order can break functionality or introduce security vulnerabilities.
