using System.IO;
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

			headers.Remove("X-Powered-By");
			headers.Remove("X-Powered-By-Plesk");
			headers.Remove("X-AspNet-Version");
			headers.Remove("ETag");

			// Some headers won't remove
			headers.Remove("Server");
			return next();
		});
	}

	[NotNull]
	public static IApplicationBuilder UseHeadMethod([NotNull] this IApplicationBuilder thisValue)
	{
		return thisValue.Use(async (ctx, next) =>
		{
			bool methodSwitched = false;

			if (HttpMethods.IsHead(ctx.Request.Method))
			{
				methodSwitched = true;
				ctx.Request.Method = HttpMethods.Get;
				ctx.Response.Body = Stream.Null;
			}

			await next(ctx);
			if (!methodSwitched) return;
			ctx.Request.Method = HttpMethods.Head;
		});
	}
}