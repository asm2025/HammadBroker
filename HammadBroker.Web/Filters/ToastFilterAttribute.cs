using System;
using System.Collections.Generic;
using HammadBroker.Extensions;
using HammadBroker.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NToastNotify;

namespace HammadBroker.Web.Filters;

public class ToastFilterAttribute : IActionFilter
{
	/// <inheritdoc />
	public void OnActionExecuting(ActionExecutingContext context) { }

	/// <inheritdoc />
	public void OnActionExecuted(ActionExecutedContext context)
	{
		HttpContext httpContext = context.HttpContext;
		ISession session = httpContext.Session;
		IList<string> errors = session.FromJson<IList<string>>(nameof(IExceptionHandler));
		if (errors == null || errors.Count == 0) return;

		IServiceProvider services = httpContext.RequestServices;
		IToastNotification toastNotification = services.GetRequiredService<IToastNotification>();

		foreach (string error in errors)
		{
			toastNotification.AddErrorToastMessage(error);
		}

		session.Remove(nameof(IExceptionHandler));
	}
}