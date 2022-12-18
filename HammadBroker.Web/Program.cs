using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using essentialMix.Core.Web.Services;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Newtonsoft.Helpers;
using essentialMix.Newtonsoft.Serialization;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Data.Repositories;
using HammadBroker.Data.Services;
using HammadBroker.Helpers;
using HammadBroker.Infrastructure.Services;
using HammadBroker.Model;
using HammadBroker.Model.Configuration;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Mail;
using HammadBroker.Model.Mapper;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace HammadBroker.Web;

public class Program
{
	private const string ARGS_SEED = "--seed";
	private const string ARGS_SEED_ = "-s";
	private const string ARGS_MIGRATE_ONLY = "--migrate-only";
	private const string ARGS_MIGRATE_ONLY_ = "-m";

	static Program()
	{
		AppName = AppDomain.CurrentDomain.FriendlyName;
		AppTitle = AppDomain.CurrentDomain.FriendlyName;
		AppPath = PathHelper.AddDirectorySeparator(Directory.GetCurrentDirectory());
		JsonConvert.DefaultSettings = () =>
		{
			JsonSerializerSettings options = JsonHelper.SetDefaults(JsonHelper.CreateSettings());
			options.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			JsonSerializerSettingsConverters allConverters = EnumHelper<JsonSerializerSettingsConverters>.GetAllFlags() &
															~(JsonSerializerSettingsConverters.IsoDateTime |
															JsonSerializerSettingsConverters.JavaScriptDateTime |
															JsonSerializerSettingsConverters.UnixDateTime);
			options.AddConverters(allConverters);
			return options;
		};
	}

	protected Program()
	{
	}

	public static string AppTitle { get; private set; }
	public static string AppName { get; private set; }
	public static string AppPath { get; }

	public static async Task Main(string[] args)
	{
		Console.OutputEncoding = Encoding.UTF8;

		// Configuration
		WebApplicationBuilder builder = CreateHostBuilder(args);
		IConfiguration configuration = builder.Configuration;
		AppName = configuration.GetValue("name", AppName);
		AppTitle = configuration.GetValue("title", AppTitle);

		ConfigureServices(builder);

		// Logging
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

		if (configuration.GetValue("LoggingEnabled", true))
		{
			loggerConfiguration.ReadFrom.Configuration(configuration)
								.Enrich.WithProperty("ApplicationName", AppName!);
		}

		Log.Logger = loggerConfiguration.CreateLogger();

		// application
		WebApplication app = builder.Build();
		Configure(app);

		ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

		try
		{
			// Start
			logger.LogInformation($"{AppName} is starting...");

			(bool migrationComplete, bool migrateOnly) = await ApplyMigrationsAsync(app, configuration, app.Environment, args, logger);

			if (!migrationComplete)
			{
				Environment.ExitCode = -1;
				return;
			}

			if (migrateOnly) return;
			logger.LogInformation("Running the application...");
			await app.RunAsync();
		}
		catch (Exception ex)
		{
			logger.LogError(ex.CollectMessages());
			Environment.ExitCode = ex.HResult;
		}
		finally
		{
			logger.LogInformation($"{AppName} finished.");
		}

		await Log.CloseAndFlushAsync();
	}

	[NotNull]
	private static WebApplicationBuilder CreateHostBuilder(string[] args)
	{
		const string WEB_ROOT = "wwwroot";

		WebApplicationOptions options = new WebApplicationOptions
		{
			ApplicationName = AppDomain.CurrentDomain.FriendlyName,
			ContentRootPath = AppPath,
			WebRootPath = WEB_ROOT,
			Args = args,
#if DEBUG
			EnvironmentName = Environments.Development
#else
			EnvironmentName = Environments.Production
#endif
		};
		WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
		IConfigurationBuilder configurationBuilder = builder.Configuration;
		configurationBuilder.Sources
							.OfType<JsonConfigurationSource>()
							.Where(e => e.Path != null && e.Path.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase))
							.ForEach(e => e.Optional = false);
		configurationBuilder.Setup(builder.Environment);
		builder.Host.UseSerilog();
		builder.WebHost.ConfigureKestrel(opt => opt.AddServerHeader = false);
		return builder;
	}

	private static void ConfigureServices([NotNull] WebApplicationBuilder builder)
	{
		IServiceCollection services = builder.Services;
		IConfiguration configuration = builder.Configuration;
		IWebHostEnvironment environment = builder.Environment;
		SmtpConfiguration smtpConfiguration = configuration.GetSection(nameof(SmtpConfiguration)).Get<SmtpConfiguration>();
		services
			// config
			.AddSingleton(configuration)
			.AddSingleton(environment)
			.AddSingleton(configuration.Get<CompanyInfo>() ?? new CompanyInfo())
			.AddVirtualPathSettings(environment.WebRootPath, opt => configuration.GetSection(nameof(VirtualPathSettings)).Bind(opt))
			// logging
			.AddLogging(config =>
			{
				config
					.AddDebug()
					.AddConsole()
					.AddSerilog();
			})
			.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
			// FormOptions
			.Configure<FormOptions>(options =>
			{
				options.ValueLengthLimit = int.MaxValue;
				options.MultipartBodyLengthLimit = int.MaxValue;
				options.MemoryBufferThreshold = int.MaxValue;
			})
			.AddHttpContextAccessor()
			.AddDefaultCorsPolicy(options => options.WithExposedHeaders("Set-Cookie"),
								(configuration.GetValue<string>("AllowedHosts").ToNullIfEmpty() ?? "*").Split(',',
																											StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			.AddForwardedHeaders()
			// Cookies
			.Configure<CookiePolicyOptions>(options =>
			{
				options.MinimumSameSitePolicy = SameSiteMode.Lax;
				options.HttpOnly = HttpOnlyPolicy.None;
				options.Secure = CookieSecurePolicy.SameAsRequest;
			})
			// Authentication
			.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.SlidingExpiration = true;
				options.LoginPath = "/Identity/Account/Login";
				options.LogoutPath = "/Identity/Account/Logout";
				options.AccessDeniedPath = "/Identity/Account/AccessDenied";
				options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("OAuth:timeout", 20).NotBelow(5));
			});
		services
			// Mapper
			.AddAutoMapper((_, options) =>
							{
								options.AddProfile(new CommonProfile());
								options.AddProfile(new IdentityProfile());
								options.AddProfile(new BuildingProfile());
							},
							new[]
							{
								typeof(CommonProfile).Assembly
							}, ServiceLifetime.Singleton)
			// Data
			.AddDbContext<DataContext>(options => DbContextHelper.Setup(options, typeof(DataContext).Assembly.GetName(), configuration, environment), ServiceLifetime.Transient)
			.AddDatabaseDeveloperPageExceptionFilter()
			// Identity
			.AddIdentity<User, Role>(options => configuration.GetSection("IdentityOptions").Bind(options))
			.AddEntityFrameworkStores<DataContext>()
			.AddDefaultUI()
			.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
			.AddUserManager<UserManager>()
			.AddRoleManager<RoleManager>()
			.AddSignInManager<SignInManager>()
			.AddRoleValidator<RoleValidator<Role>>()
			.AddDefaultTokenProviders();
		services
			.AddScoped(typeof(IUserClaimsPrincipalFactory<User>), typeof(UserClaimsPrincipalFactory))
			.AddScoped(typeof(UserManager<User>), typeof(UserManager))
			.AddScoped(typeof(RoleManager<Role>), typeof(RoleManager))
			.AddScoped(typeof(SignInManager<User>), typeof(SignInManager))
			// Repositories
			.AddTransient<ICityRepository, CityRepository>()
			.AddTransient<IIdentityRepository, IdentityRepository>()
			.AddTransient<IBuildingRepository, BuildingRepository>()
			.AddTransient<IBuildingAdRepository, BuildingAdRepository>()
			// Services
			.AddTransient<IUploaderService, UploaderService>()
			.AddTransient<ILookupService, LookupService>()
			.AddTransient<ICityService, CityService>()
			.AddTransient<IIdentityService, IdentityService>()
			.AddTransient<IBuildingService, BuildingService>()
			.AddTransient<IBuildingAdService, BuildingAdService>()
			// Authorization
			.AddAuthorization(options =>
			{
				options.AddPolicy(Constants.Authorization.MemberPolicy, policy =>
				{
					policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
						.RequireAuthenticatedUser();
				});

				options.AddPolicy(Constants.Authorization.AdministrationPolicy, policy =>
				{
					policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
						.RequireRole(Role.System, Role.Administrators);
				});

				options.AddPolicy(Constants.Authorization.SystemPolicy, policy =>
				{
					policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
						.RequireRole(Role.System);
				});
			})
			// MVC
			.AddControllersWithViews();
		services
			// Razor Pages
			.AddRazorPages()
			// JSON
			.AddNewtonsoftJson(options =>
			{
				JsonHelper.SetDefaults(options.SerializerSettings, contractResolver: new CamelCasePropertyNamesContractResolver());
				options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

				JsonSerializerSettingsConverters allConverters = EnumHelper<JsonSerializerSettingsConverters>.GetAllFlags() &
																~(JsonSerializerSettingsConverters.IsoDateTime |
																JsonSerializerSettingsConverters.JavaScriptDateTime |
																JsonSerializerSettingsConverters.UnixDateTime);
				options.SerializerSettings.AddConverters(allConverters);
			});

		if (smtpConfiguration != null && !string.IsNullOrWhiteSpace(smtpConfiguration.Host))
		{
			// Add email senders which is currently setup for SMTP
			services
				.AddSingleton(smtpConfiguration)
				.AddTransient<IEmailService, SmtpEmailService>();
		}
	}

	private static void Configure([NotNull] WebApplication app)
	{
		IConfiguration configuration = app.Configuration;
		IWebHostEnvironment environment = app.Environment;
		bool isDevelopmentOrStaging = environment.IsDevelopment() || environment.IsStaging();

		// Configure the HTTP request pipeline.
		if (isDevelopmentOrStaging)
			app.UseMigrationsEndPoint();
		else
			app.UseExceptionHandler("/Error");


		if (!environment.IsDevelopment() || configuration.GetValue<bool>("useSSL"))
		{
			app.UseHsts(options =>
				{
					options.Preload()
							.IncludeSubdomains()
							.MaxAge(365);
				})
				.UseHttpsRedirection();
		}

		app
			.UseForwardedHeaders()
			.UseSerilogRequestLogging()
			.UseDefaultFiles()
			.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = environment.WebRootFileProvider
			})
			.UseVirtualPathSettings(environment.WebRootPath, environment.WebRootFileProvider)
			.UseRouting()
			// Add custom security headers
			//.UseSecurityHeaders(configuration)
			.UseAuthentication()
			.UseAuthorization()
			// last
			.UseVirtualPathEndpoints(environment.WebRootPath)
			.UseEndpoints(endpoint =>
			{
				endpoint.MapAreaControllerRoute(nameof(Areas.Admin), nameof(Areas.Admin), $"{nameof(Areas.Admin)}/{{controller=Home}}/{{action=Index}}/{{id?}}")
						.RequireAuthorization(Constants.Authorization.AdministrationPolicy);
				endpoint.MapControllers();
				endpoint.MapRazorPages();
				endpoint.MapDefaultControllerRoute();
			});
	}

	private static async Task<(bool, bool)> ApplyMigrationsAsync([NotNull] IHost host, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, string[] args, [NotNull] ILogger logger)
	{
		bool applyMigrations = configuration.GetValue("Migrations:ApplyMigrations", true);
		bool applySeed = configuration.GetValue("Migrations:ApplySeed", true);
		bool migrateOnly = false;

		if (args is { Length: > 0 })
		{
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;

			foreach (string arg in args)
			{
				if (comparer.Equals(arg, ARGS_MIGRATE_ONLY) || comparer.Equals(arg, ARGS_MIGRATE_ONLY_))
					migrateOnly = true;
				else if (comparer.Equals(arg, ARGS_SEED) || comparer.Equals(arg, ARGS_SEED_))
					applySeed = true;
			}

			applyMigrations |= migrateOnly;
		}

		if (!applyMigrations) return (true, false);
		logger.LogInformation("Checking database migrations...");

		IServiceScope scope = null;
		DataContext dataContext = null;

		try
		{
			scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
			dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

			bool isMigrated = await dataContext.IsMigratedAsync();

			if (isMigrated)
			{
				logger.LogInformation("Database is migrated.");
				return (true, migrateOnly);
			}

			IConfiguration seedConfiguration = new ConfigurationBuilder()
												.SetBasePath(AppPath)
												.AddConfigurationFile(AppPath, "seed.json", false, environment.EnvironmentName)
												.Build();

			if (!await dataContext.ApplyMigrationsAsync(host, seedConfiguration, applySeed, logger))
			{
				logger.LogError("Database migrations were not successful.");
				return (false, migrateOnly);
			}

			logger.LogInformation("Database migrations completed successfully.");
			return (true, migrateOnly);
		}
		finally
		{
			ObjectHelper.Dispose(ref dataContext);
			ObjectHelper.Dispose(ref scope);
		}
	}
}