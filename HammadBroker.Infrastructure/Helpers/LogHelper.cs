using System.IO;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Helpers;

public static class LogHelper
{
	public const string OUTPUT_FORMAT = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}    {Properties:j}{NewLine}";

	[NotNull]
	public static ILogger<T> CreateStartLogger<T>(string path = null, string appName = null)
	{
		appName = appName.ToNullIfEmpty() ?? typeof(T).Name;
		path = path.ToNullIfEmpty() ?? $"{appName}-.log";

		Logger logger = new LoggerConfiguration()
												.MinimumLevel.Debug()
												.Enrich.FromLogContext()
												.Enrich.WithProperty("Application", appName)
												.Enrich.With(new MachineNameEnricher(), new EnvironmentUserNameEnricher())
												.WriteTo.Console()
												.WriteTo.Debug()
												.WriteTo.File(path, LogEventLevel.Debug, OUTPUT_FORMAT, null, int.MaxValue, null, false, true, TimeSpanHelper.Second,
															RollingInterval.Day, true)
												.CreateLogger();
		SerilogLoggerFactory factory = new SerilogLoggerFactory(logger);
		return factory.CreateLogger<T>();
	}

	[NotNull]
	public static IConfiguration SetPath([NotNull] IConfiguration configuration, int position, string environmentName)
	{
		const string CONFIG_PATH = "Serilog:WriteTo:{0}:Args:path";

		if (string.IsNullOrEmpty(environmentName)) return configuration;

		string configPath = string.Format(CONFIG_PATH, position);
		string path = configuration.GetValue<string>(configPath)?.Trim('\\', ' ');
		if (string.IsNullOrEmpty(path)) return configuration;

		int index = path.LastIndexOf('\\');
		string fileName = path;

		if (index < 1)
		{
			path = EnvironmentHelper.GetName();
		}
		else
		{
			fileName = path[(index + 1)..];
			path = Path.Combine(path[..index], EnvironmentHelper.GetName());
		}

		configuration[configPath] = Path.Combine(path, fileName);
		return configuration;
	}
}