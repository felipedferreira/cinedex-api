namespace Cinedex.Web.Constants;

public static class AuthenticationConstants
{
    /// <summary>
    /// The cookie name used to store the refresh token on the client.
    ///
    /// This cookie is HttpOnly and Secure to prevent JavaScript access and ensure
    /// it's only transmitted over HTTPS. The refresh token is used to obtain new
    /// access tokens without requiring the user to re-authenticate.
    /// </summary>
    public const string RefreshTokenCookie = "RT";
}
