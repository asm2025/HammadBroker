using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Infrastructure.Middleware;

public interface IExceptionHandler
{
	bool OnError([NotNull] HttpContext context, [NotNull] Exception exception);
}