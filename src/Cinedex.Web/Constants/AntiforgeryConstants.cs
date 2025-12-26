namespace Cinedex.Web.Constants;

public static class AntiforgeryConstants
{
    /// <summary>
    /// The HTTP request header used to transmit the anti-forgery (XSRF/CSRF) token
    /// from the client to the server.
    ///
    /// This header is typically populated by JavaScript (e.g., a SPA) by reading
    /// the corresponding XSRF cookie and echoing its value back to the server.
    /// ASP.NET Core validates this header against the anti-forgery cookie to
    /// protect against Cross-Site Request Forgery (CSRF) attacks.
    /// </summary>
    public const string XsrfHeader = "X-XSRF-TOKEN";
    
    /// <summary>
    /// The cookie name used to store the anti-forgery (XSRF/CSRF) token on the client.
    ///
    /// This cookie is intentionally readable by JavaScript (not HttpOnly) so that
    /// Single Page Applications (SPAs) can retrieve its value and include it in
    /// the corresponding XSRF request header.
    /// The server validates that the cookie value matches the XSRF header value
    /// to ensure the request originated from the same site.
    /// </summary>
    public const string XsrfCookie = "XSRF-TOKEN";
}