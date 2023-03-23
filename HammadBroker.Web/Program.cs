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
using HammadBroker.Extensions;
using HammadBroker.Helpers;
using HammadBroker.Infrastructure.Middleware;
using HammadBroker.Infrastructure.Services;
using HammadBroker.Model;
using HammadBroker.Model.Configuration;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Mail;
using HammadBroker.Model.Mapper;
using HammadBroker.Model.VirtualPath;
using HammadBroker.Web.Filters;
using HammadBroker.Web.Middleware;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NToastNotify;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using SLogger = Serilog.ILogger;

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
		AppName = configuration.GetValue("AppName", AppName);
		AppTitle = configuration.GetValue("Name", AppTitle);

		// Logging
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

		if (configuration.GetValue("LoggingEnabled", true))
		{
			loggerConfiguration.ReadFrom.Configuration(configuration)
								.Enrich.WithProperty("ApplicationName", AppName!);
		}

		Log.Logger = loggerConfiguration.CreateLogger();
		SLogger logger = Log.Logger;

		ConfigureServices(builder);

		// application
		WebApplication app = builder.Build();
		Configure(app);

		try
		{
			// Start
			logger.Information($"{AppName} is starting...");

			(bool migrationComplete, bool migrateOnly) = await ApplyMigrationsAsync(app, configuration, app.Environment, args, logger);

			if (!migrationComplete)
			{
				Environment.ExitCode = -1;
				return;
			}

			if (migrateOnly) return;
			logger.Information("Running the application...");
			await app.RunAsync();
		}
		catch (Exception ex)
		{
			logger.Error(ex.CollectMessages());
			Environment.ExitCode = ex.HResult;
		}
		finally
		{
			logger.Information($"{AppName} finished.");
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
		CompanyInfo companyInfo = configuration.Get<CompanyInfo>() ?? new CompanyInfo();
		int sessionTimeout = configuration.GetValue("OAuth:timeout", 20).NotBelow(5);
		services
			// config
			.AddSingleton(configuration)
			.AddSingleton(environment)
			.AddSingleton(companyInfo)
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
				options.ValueCountLimit = int.MaxValue;
				options.MultipartBodyLengthLimit = int.MaxValue;
				options.MemoryBufferThreshold = int.MaxValue;
			})
			.AddHttpContextAccessor()
			.AddDefaultCorsPolicy(options => options.WithExposedHeaders("Set-Cookie"),
								(configuration.GetValue<string>("AllowedHosts").ToNullIfEmpty() ?? "*").Split(',',
																											StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			.AddForwardedHeaders()
			.AddSingleton<IExceptionHandler, ExceptionHandler>()
			// Cookies
			.Configure<CookiePolicyOptions>(options =>
			{
				options.HttpOnly = HttpOnlyPolicy.Always;
				options.Secure = CookieSecurePolicy.SameAsRequest;
				options.MinimumSameSitePolicy = SameSiteMode.Lax;
			})
			.AddDistributedMemoryCache()
			.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeout);
				options.Cookie.IsEssential = true;
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
				options.Cookie.SameSite = SameSiteMode.Lax;
			})
			// Authentication
			.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.LoginPath = "/Identity/Account/Login";
				options.LogoutPath = "/Identity/Account/Logout";
				options.AccessDeniedPath = "/Identity/Account/AccessDenied";
				options.Cookie.IsEssential = true;
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
				options.Cookie.SameSite = SameSiteMode.Lax;
			});
		// Data protection
		services
			.AddDataProtection()
			.PersistKeysToDbContext<DataContext>()
			.SetDefaultKeyLifetime(TimeSpan.FromDays(90));
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
			.AddIdentity<User, Role>(options => configuration.GetSection(nameof(IdentityOptions)).Bind(options))
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
			.AddTransient<IDistrictRepository, DistrictRepository>()
			.AddTransient<ICityRepository, CityRepository>()
			.AddTransient<IIdentityRepository, IdentityRepository>()
			.AddTransient<IBuildingRepository, BuildingRepository>()
			// Services
			.AddTransient<IUploaderService, UploaderService>()
			.AddTransient<ILookupService, LookupService>()
			.AddTransient<IDistrictService, DistrictService>()
			.AddTransient<ICityService, CityService>()
			.AddTransient<IIdentityService, IdentityService>()
			.AddTransient<IBuildingService, BuildingService>()
			// Authorization
			.AddAuthorization(options =>
			{
				options.AddPolicy(Constants.Authorization.MemberPolicy, policy => policy.RequireRole(Role.Administrators, Role.Members));
				options.AddPolicy(Constants.Authorization.AdministrationPolicy, policy => policy.RequireRole(Role.Administrators));
			})
			// MVC
			.AddControllersWithViews(options =>
			{
				options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
				options.Filters.Add(new ToastFilterAttribute());
			})
			.ConfigureApiBehaviorOptions(options =>
			{
				options.SuppressConsumesConstraintForFormFileParameters = true;
				options.SuppressInferBindingSourcesForParameters = true;
				options.SuppressModelStateInvalidFilter = true;
				options.SuppressMapClientErrors = true;
			});
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
			})
			.AddSessionStateTempDataProvider()
			.AddNToastNotifyToastr(configuration.GetSection(nameof(ToastrOptions)).Get<ToastrOptions>());

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
			.UseSession()
			.UseExceptionMiddleware()
			.UseNToastNotify()
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

	private static async Task<(bool, bool)> ApplyMigrationsAsync([NotNull] IHost host, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, string[] args, SLogger logger)
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
		logger.Information("Checking database migrations...");

		IServiceScope scope = null;
		DataContext dataContext = null;

		try
		{
			scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
			IServiceProvider services = scope.ServiceProvider;

			dataContext = services.GetRequiredService<DataContext>();
			bool isMigrated = await dataContext.IsMigratedAsync();

			if (isMigrated)
			{
				logger.Information("Database is migrated.");
				return (true, migrateOnly);
			}

			IConfiguration seedConfiguration = new ConfigurationBuilder()
												.SetBasePath(AppPath)
												.AddConfigurationFile(AppPath, "seed.json", false, environment.EnvironmentName)
												.Build();
			ILogger xLogger = services.GetService<ILogger<DataContext>>();

			if (!await dataContext.ApplyMigrationsAsync(host, seedConfiguration, applySeed, xLogger))
			{
				logger.Error("Database migrations were not successful.");
				return (false, migrateOnly);
			}

			logger.Information("Database migrations completed successfully.");
			return (true, migrateOnly);
		}
		finally
		{
			ObjectHelper.Dispose(ref dataContext);
			ObjectHelper.Dispose(ref scope);
		}
	}
}