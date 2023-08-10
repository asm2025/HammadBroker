using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Extensions;

public static class IApplicationBuilderExtension
{

	[NotNull]
	public static IApplicationBuilder UseCustomHeaders([NotNull] this IApplicationBuilder thisValue)
	{
		return thisValue.Use((ctx, next) =>
		{
			IHeaderDictionary headers = ctx.Response.Headers;
			headers["X-Frame-Options"] = "DENY";
			headers["X-XSS-Protection"] = "1; mode=block";
			headers["X-Content-Type-Options"] = "nosniff";
			headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
			headers["Allow"] = "*";
			headers["Access-Control-Allow-Methods"] = "*";

			headers.Remove("X-Powered-By");
			headers.Remove("X-Powered-By-Plesk");
			headers.Remove("X-AspNet-Version");
			headers.Remove("ETag");

			// Some headers won't remove
			headers.Remove("Server");
			return next();
		});
	}
}