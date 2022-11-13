using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Helpers;

public static class AuthenticationHelper
{
	public static void CheckSameSite(HttpContext httpContext, CookieOptions options)
	{
		if (options.SameSite != SameSiteMode.None) return;

		string userAgent = httpContext.Request.Headers["User-Agent"].ToString();
		if (httpContext.Request.IsHttps && !DisallowsSameSiteNone(userAgent)) return;
		options.SameSite = SameSiteMode.Unspecified;
	}

	public static bool DisallowsSameSiteNone(string userAgent)
	{
		return userAgent.Contains("CPU iPhone OS 12") ||
				userAgent.Contains("iPad; CPU OS 12") ||
				userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/") && userAgent.Contains("Safari") ||
				userAgent.Contains("Chrome/5") ||
				userAgent.Contains("Chrome/6");
	}
}