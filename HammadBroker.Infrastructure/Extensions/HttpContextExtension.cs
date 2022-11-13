using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Extensions;

public static class HttpContextExtension
{
	public static void CheckSameSite(this HttpContext thisValue, [NotNull] CookieOptions options)
	{
		if (thisValue == null || options.SameSite != SameSiteMode.None) return;
		string userAgent = thisValue.Request.Headers["User-Agent"].ToString();
		if (thisValue.Request.IsHttps && !DisallowsSameSiteNone(userAgent)) return;
		options.SameSite = SameSiteMode.Unspecified;
	}

	private static bool DisallowsSameSiteNone(string userAgent)
	{
		return !string.IsNullOrWhiteSpace(userAgent) &&
				(userAgent.Contains("CPU iPhone OS 12") ||
				userAgent.Contains("iPad; CPU OS 12") ||
				userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/") && userAgent.Contains("Safari") ||
				userAgent.Contains("Chrome/5") ||
				userAgent.Contains("Chrome/6"));
	}
}