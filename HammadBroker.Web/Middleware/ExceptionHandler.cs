using System;
using AspNetCoreHero.ToastNotification.Abstractions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Infrastructure.Middleware;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Middleware;

public class ExceptionHandler : IExceptionHandler
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<ExceptionHandler> _logger;

	public ExceptionHandler([NotNull] IServiceScopeFactory serviceScopeFactory, [NotNull] ILogger<ExceptionHandler> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	/// <inheritdoc />
	public bool Handle(HttpContext context, Exception exception)
	{
		_logger.LogError(exception.CollectMessages());

		IServiceScope scope = null;

		try
		{
			scope = _serviceScopeFactory.CreateScope();
			IToastifyService toastifyService = scope.ServiceProvider.GetRequiredService<IToastifyService>();
			toastifyService.Error(exception.Unwrap());
		}
		catch
		{
			return false;
		}
		finally
		{
			ObjectHelper.Dispose(ref scope);
		}

		return true;
	}
}