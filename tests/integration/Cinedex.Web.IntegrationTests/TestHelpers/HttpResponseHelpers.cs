namespace Cinedex.Web.IntegrationTests.TestHelpers;

public static class HttpResponseHelpers
{
    public static string? ExtractCookie(HttpResponseMessage response, string cookieName)
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
