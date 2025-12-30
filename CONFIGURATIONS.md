# ⚙️ Configuration Guide

This document explains the various configuration options available in Cinedex and their implications.

---

## 📍 Configuration Files

Cinedex uses ASP.NET Core's configuration system, which merges settings from multiple sources in order:

1. `appsettings.json` - Base configuration for all environments
2. `appsettings.{Environment}.json` - Environment-specific overrides (e.g., `appsettings.Development.json`)
3. **User Secrets** (Development only) - Sensitive data stored outside the project
4. **Environment Variables** - System-level configuration
5. **Command-line arguments** - Runtime overrides

---

## 🌍 Environment Configuration

### `ASPNETCORE_ENVIRONMENT`

Controls which environment the application runs in. Set via environment variable.

**Possible Values:**
- `Development`
- `Staging`
- `Production`

**Implications:**

| Feature | Development | Production |
|---------|-------------|------------|
| **Detailed Error Pages** | ✅ Enabled (shows stack traces) | ❌ Disabled (shows generic errors) |
| **Scalar API Documentation** | ✅ Enabled at `/scalar/v1` | ❌ Disabled |
| **OpenAPI Endpoint** | ✅ Enabled | ❌ Disabled |
| **Developer Exception Page** | ✅ Enabled | ❌ Disabled |
| **Logging Verbosity** | 🔊 More verbose | 🔇 Less verbose |
| **User Secrets** | ✅ Loaded | ❌ Not loaded |

**How to Set:**

```bash
# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Windows (Command Prompt)
set ASPNETCORE_ENVIRONMENT=Development

# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Development

# launchSettings.json (for Visual Studio/Rider)
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

---

## 🔒 JWT Authentication

JWT (JSON Web Token) configuration for access token generation and validation.

### Configuration

```json
{
  "Jwt": {
    "Issuer": "https://www.felipe-ferreira.codes/movie-svc",
    "Audience": "movie-svc",
    "Secret": "<secret-key-placeholder>"
  }
}
```

### Properties

| Property | Description | Example |
|----------|-------------|---------|
| **Issuer** | The issuer of the token (usually your domain/service) | `"https://www.felipe-ferreira.codes/movie-svc"` |
| **Audience** | The intended audience/consumer of the token | `"movie-svc"` |
| **Secret** | Secret key used to sign and validate tokens | `"your-256-bit-secret-key-here"` |

### Security Considerations

⚠️ **IMPORTANT:** Never commit your JWT secret to source control!

**Best Practices:**

1. **Development:** Use User Secrets
   ```bash
   dotnet user-secrets set "Jwt:Secret" "your-development-secret-key"
   ```

2. **Production:** Use Environment Variables or Azure Key Vault
   ```bash
   # Environment Variable
   export Jwt__Secret="your-production-secret-key"
   ```

3. **Secret Requirements:**
   - Minimum 256 bits (32 characters) for HS256 algorithm
   - Use cryptographically secure random generation
   - Rotate secrets periodically

**Generating a Secure Secret:**

```bash
# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Min 0 -Max 256 }))

# Linux/macOS
openssl rand -base64 32
```

---

## 🌐 CORS (Cross-Origin Resource Sharing)

Controls which web origins can access your API.

### Configuration

```json
{
  "CORS": {
    "Enabled": false,
    "AllowedOrigins": []
  }
}
```

### Properties

| Property | Description | Default |
|----------|-------------|---------|
| **Enabled** | Enable/disable CORS middleware | `false` |
| **AllowedOrigins** | Array of allowed origins | `[]` (empty) |

### Examples

**Single Origin (Development):**
```json
{
  "CORS": {
    "Enabled": true,
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

**Multiple Origins (Production):**
```json
{
  "CORS": {
    "Enabled": true,
    "AllowedOrigins": [
      "https://yourdomain.com",
      "https://www.yourdomain.com",
      "https://app.yourdomain.com"
    ]
  }
}
```

### Security Considerations

⚠️ **NEVER** use `"*"` (wildcard) in production with credentials!

**Current CORS Policy:**
- ✅ `AllowCredentials()` - Allows cookies and authentication headers
- ✅ `AllowAnyHeader()` - Permits all request headers
- ✅ `AllowAnyMethod()` - Permits all HTTP methods (GET, POST, PUT, DELETE, etc.)

**When to Enable CORS:**
- ✅ Frontend SPA (React, Vue, Angular) on different domain
- ✅ Mobile app making requests from web view
- ❌ API-to-API communication (not needed)
- ❌ Same-origin requests (not needed)

---

## 🍪 Cookie Configuration

Cookies are used for refresh tokens and XSRF protection.

### Refresh Token Cookie (`RT`)

**Configuration in Code:**
```csharp
httpContext.Response.Cookies.Append(
    AuthenticationConstants.RefreshTokenCookie, // "RT"
    refreshToken,
    new CookieOptions
    {
        HttpOnly = true,        // Prevents JavaScript access
        Secure = true,          // HTTPS only
        SameSite = SameSiteMode.Strict,
        Path = "/movie-svc/refresh",
        MaxAge = TimeSpan.FromDays(7)
    });
```

**Properties:**
- **Name:** `RT` (short for Refresh Token)
- **HttpOnly:** `true` - Cannot be accessed via JavaScript (XSS protection)
- **Secure:** `true` - Only sent over HTTPS
- **SameSite:** `Strict` - CSRF protection
- **Path:** `/movie-svc/refresh` - Only sent to refresh endpoint
- **MaxAge:** 7 days

### XSRF Token Cookie (`XSRF-TOKEN`)

**Configuration in Code:**
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false;  // Readable by JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
});
```

**Properties:**
- **Name:** `XSRF-TOKEN`
- **Header Name:** `X-XSRF-TOKEN`
- **HttpOnly:** `false` - Must be readable by JavaScript for SPAs
- **Secure:** `Always` - HTTPS only
- **SameSite:** `Strict`
- **Path:** `/` - Available across all endpoints

**How It Works:**
1. Client requests `/csrf` endpoint
2. Server generates token and stores in `XSRF-TOKEN` cookie
3. Client reads cookie value via JavaScript
4. Client includes value in `X-XSRF-TOKEN` header for state-changing requests (POST, PUT, DELETE)
5. Server validates header matches cookie

---

## 🛣️ Path Prefix

All API endpoints are prefixed with `/movie-svc`.

**Example:**
- ✅ `https://api.example.com/movie-svc/login`
- ✅ `https://api.example.com/movie-svc/movies`
- ❌ `https://api.example.com/login` (redirects to `/movie-svc/login`)

**Automatic Redirect:**
Requests without the prefix are automatically redirected with HTTP 301 (Permanent Redirect).

**Configuration:**
```csharp
// Constant defined in PathConstants.cs
public const string ApiBasePath = "/movie-svc";
```

---

## 📝 Logging Configuration

Cinedex uses Serilog for structured logging.

### Current Configuration

```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Literate);
});
```

### Log Levels

Configure in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    }
  }
}
```

**Available Levels:**
- `Verbose` - Extremely detailed (rarely used)
- `Debug` - Debugging information
- `Information` - General informational messages
- `Warning` - Warnings that don't stop execution
- `Error` - Errors and exceptions
- `Fatal` - Critical failures

---

## 🔐 Security Checklist

Before deploying to production:

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Use secure JWT secret (256+ bits) from Key Vault or environment variable
- [ ] Configure `AllowedOrigins` with specific domains (no wildcards)
- [ ] Ensure HTTPS is enforced (`Secure` cookies, `UseHttpsRedirection`)
- [ ] Review and minimize logging verbosity
- [ ] Enable CORS only if needed
- [ ] Rotate JWT secrets periodically
- [ ] Use separate configuration for each environment

---

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [User Secrets in Development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [Serilog Documentation](https://serilog.net/)
