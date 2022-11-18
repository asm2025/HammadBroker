using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
using HammadBroker.Infrastructure.Services;
using HammadBroker.Model.Configuration;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Mail;
using HammadBroker.Model.Mapper;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
	public static string AppPath { get; }

	public static async Task Main(string[] args)
	{
		Console.OutputEncoding = Encoding.UTF8;

		// Configuration
		WebApplicationBuilder builder = CreateHostBuilder(args);
		IConfiguration configuration = builder.Configuration;
		AppTitle = configuration.GetValue("title", AppTitle);

		ConfigureServices(builder);

		// Logging
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

		if (configuration.GetValue("LoggingEnabled", true))
		{
			loggerConfiguration.ReadFrom.Configuration(configuration)
								.Enrich.WithProperty("ApplicationName", AppTitle!);
		}

		Log.Logger = loggerConfiguration.CreateLogger();

		// application
		WebApplication app = builder.Build();
		Configure(app);

		ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

		try
		{
			// Start
			logger.LogInformation($"{AppTitle} is starting...");

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

			bool migrationComplete = true;

			if (applyMigrations)
			{
				logger.LogInformation("Checking database migrations...");

				IServiceScope scope = null;
				DataContext dataContext = null;

				try
				{
					scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
					dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

					bool isMigrated = await dataContext.IsMigratedAsync();

					if (isMigrated)
						logger.LogInformation("Database is migrated.");
					else
						migrationComplete = await dataContext.ApplyMigrationsAsync(app, applySeed, logger);
				}
				finally
				{
					ObjectHelper.Dispose(ref dataContext);
					ObjectHelper.Dispose(ref scope);
				}
			}

			if (migrateOnly)
			{
				if (!migrationComplete)
				{
					logger.LogError("Database migrations were not successful.");
					Environment.ExitCode = -1;
				}
				else
				{
					logger.LogError("Database migrations completed successfully.");
				}

				return;
			}

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
			logger.LogInformation($"{AppTitle} finished.");
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
		bool isDevelopmentOrStaging = environment.IsDevelopment() || environment.IsStaging();
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
			.Configure<CookiePolicyOptions>(options =>
			{
				options.MinimumSameSitePolicy = SameSiteMode.Lax;
				options.Secure = CookieSecurePolicy.SameAsRequest;
				options.OnAppendCookie = context => context.Context.CheckSameSite(context.CookieOptions);
				options.OnDeleteCookie = context => context.Context.CheckSameSite(context.CookieOptions);
			})
			// OAuth
			.AddJwtBearerAuthentication()
			.AddCookie(options =>
			{
				options.SlidingExpiration = true;
				options.LoginPath = "/account/login";
				options.LogoutPath = "/account/logout";
				options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("OAuth:timeout", 20).NotBelow(5));
			})
			.AddJwtBearerOptions(options =>
			{
				SecurityKey signingKey = SecurityKeyHelper.CreateSymmetricKey(configuration.GetValue<string>("OAuth:signingKey"), 256);
				//SecurityKey decryptionKey = SecurityKeyHelper.CreateSymmetricKey(configuration.GetValue<string>("OAuth:encryptionKey"), 256);
				options.Setup(signingKey, /*decryptionKey, */configuration, isDevelopmentOrStaging);
			});
		services
			// Helpers
			.AddHttpContextAccessor()
			.AddDefaultCorsPolicy(options => options.WithExposedHeaders("Set-Cookie"), (configuration.GetValue<string>("AllowedHosts").ToNullIfEmpty() ?? "*").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			.AddForwardedHeaders()
			// Mapper
			.AddAutoMapper((_, options) =>
							{
								options.AddProfile(new CommonProfile());
								options.AddProfile(new IdentityProfile());
							},
							new[]
							{
								typeof(CommonProfile).Assembly
							}, ServiceLifetime.Singleton)
			// Data
			.AddDbContext<DataContext>(options => DbContextHelper.Setup(options, typeof(DataContext).Assembly.GetName(), configuration, environment), ServiceLifetime.Transient)
			.AddDatabaseDeveloperPageExceptionFilter()
			// Identity
			.AddIdentity<ApplicationUser, ApplicationRole>(options => configuration.GetSection("IdentityOptions").Bind(options))
			.AddEntityFrameworkStores<DataContext>()
			.AddDefaultUI()
			.AddUserManager<UserManager>()
			.AddRoleManager<RoleManager>()
			.AddSignInManager<SignInManager>()
			.AddRoleValidator<RoleValidator<ApplicationRole>>()
			.AddDefaultTokenProviders();
		services
			.AddScoped(typeof(UserManager<ApplicationUser>), typeof(UserManager))
			.AddScoped(typeof(RoleManager<ApplicationRole>), typeof(RoleManager))
			.AddScoped(typeof(SignInManager<ApplicationUser>), typeof(SignInManager))
			// Repositories
			.AddTransient<IIdentityRepository, IdentityRepository>()
			.AddTransient<IBuildingRepository, BuildingRepository>()
			.AddTransient<IAdRepository, AdRepository>()
			// Services
			.AddTransient<IUploaderService, UploaderService>()
			.AddTransient<IIdentityService, IdentityService>()
			.AddTransient<IBuildingService, BuildingService>()
			.AddTransient<IAdService, AdService>()
			.AddTransient<ILookupService, LookupService>()
			// Authorization
			.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
										.RequireAuthenticatedUser()
										.RequireClaim(ClaimTypes.Role, ApplicationRole.Roles.Keys)
										.Build();

				options.AddPolicy(ApplicationRole.Members, policy =>
				{
					policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
						  .RequireAuthenticatedUser()
						  .RequireClaim(ClaimTypes.Role, ApplicationRole.Members)
						  .RequireRole(ApplicationRole.Members);
				});

				options.AddPolicy(ApplicationRole.Administrators, policy =>
				{
					policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
						  .RequireAuthenticatedUser()
						  .RequireClaim(ClaimTypes.Role, ApplicationRole.Administrators)
						  .RequireRole(ApplicationRole.Administrators);
				});

				options.AddPolicy(ApplicationRole.System, policy =>
				{
					policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
						  .RequireAuthenticatedUser()
						  .RequireClaim(ClaimTypes.Role, ApplicationRole.System)
						  .RequireRole(ApplicationRole.System);
				});
			});
		// MVC
		services
			.AddControllersWithViews();
		services
			.AddRazorPages()
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
				.AddTransient<IEmailService, SmtpEmailService>()
				.AddTransient(typeof(IEmailSender), typeof(IEmailService));
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
			.UseCookiePolicy(new CookiePolicyOptions
			{
				MinimumSameSitePolicy = SameSiteMode.Lax,
				HttpOnly = HttpOnlyPolicy.None,
				Secure = CookieSecurePolicy.SameAsRequest,
				OnAppendCookie = cookieContext => AuthenticationHelper.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions),
				OnDeleteCookie = cookieContext => AuthenticationHelper.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions)
			})
			.UseRouting()
			// Add custom security headers
			.UseSecurityHeaders(configuration)
			.UseAuthentication()
			.UseAuthorization()
			// last
			.UseVirtualPathEndpoints(environment.WebRootPath)
			.UseEndpoints(endpoint =>
			{
				endpoint.MapRazorPages();
				endpoint.MapControllers();
				endpoint.MapDefaultControllerRoute();
			});
	}
}