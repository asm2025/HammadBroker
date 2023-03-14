using System;
using System.Collections.Generic;
using essentialMix.Extensions;
using HammadBroker.Extensions;
using HammadBroker.Infrastructure.Middleware;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Middleware;

public class ExceptionHandler : IExceptionHandler
{
	private readonly ILogger<ExceptionHandler> _logger;

	public ExceptionHandler([NotNull] ILogger<ExceptionHandler> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public bool OnError(HttpContext context, Exception exception)
	{
		_logger.LogError(exception.CollectMessages());

		ISession session = context.Session;
		IList<string> errors = session.FromJson<IList<string>>(nameof(IExceptionHandler)) ?? new List<string>();
		errors.Add(exception.Unwrap());
		session.ToJson(nameof(IExceptionHandler), errors);
		return true;
	}
}