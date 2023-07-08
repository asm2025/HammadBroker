using System;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Helpers;

public static class EnvironmentHelper
{
	private static readonly string __defaultEnvironmentName;

	static EnvironmentHelper()
	{
#if DEBUG
		__defaultEnvironmentName = Environments.Development;
#else
		__defaultEnvironmentName = Environments.Production;
#endif
	}

	[NotNull]
	public static string GetName()
	{
		return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT").ToNullIfEmpty() ??
				Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToNullIfEmpty() ??
				__defaultEnvironmentName;
	}
}