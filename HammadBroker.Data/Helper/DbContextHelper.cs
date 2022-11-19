using System;
using System.Reflection;
using essentialMix.Extensions;
using essentialMix.Logging.Helpers;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Helpers;

public static class DbContextHelper
{
	public static void Setup([NotNull] DbContextOptionsBuilder options, [NotNull] AssemblyName migrationAssemblyName, [NotNull] IConfiguration configuration, [NotNull] IHostEnvironment environment)
	{
		if (options.IsConfigured) return;

		string connectionString = configuration.GetConnectionString("DefaultConnection").ToNullIfEmpty() ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		bool isDevelopmentOrStaging = environment.IsDevelopment() || environment.IsStaging();

		if (configuration.GetValue("LoggingEnabled", true))
		{
			options
				.UseLoggerFactory(LogFactoryHelper.ConsoleLoggerFactory)
				.EnableSensitiveDataLogging(configuration.GetValue("DataLoggingEnabled", isDevelopmentOrStaging));
		}

		options
			.UseLazyLoadingProxies()
			.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationAssemblyName.Name))
			.EnableDetailedErrors(configuration.GetValue("DetailedErrors", isDevelopmentOrStaging));
	}
}