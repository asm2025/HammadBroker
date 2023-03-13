using System;
using System.Threading.Tasks;
using HammadBroker.Infrastructure.Middleware;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Infrastructure.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IExceptionHandler _handler;

		public ExceptionMiddleware([NotNull] RequestDelegate next, [NotNull] IExceptionHandler handler)
		{
			_next = next;
			_handler = handler;
		}

		public async Task Invoke([NotNull] HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				if (_handler.Handle(context, ex)) return;
				throw;
			}
		}
	}
}

namespace HammadBroker.Extensions
{
	public static class ExceptionMiddlewareExtension
	{
		[NotNull]
		public static IApplicationBuilder UseExceptionMiddleware([NotNull] this IApplicationBuilder thisValue)
		{
			return thisValue.UseMiddleware<ExceptionMiddleware>();
		}
	}
}