using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Helpers;
using HammadBroker.Model;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Context;

public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
	IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,
	IdentityRoleClaim<string>, IdentityUserToken<string>>
{

	/// <inheritdoc />
	public DataContext()
	{
	}

	public DataContext(DbContextOptions<DataContext> options)
		: base(options)
	{
	}

	public DbSet<Country> Countries { get; set; }
	public DbSet<City> Cities { get; set; }
	public DbSet<BuildingType> BuildingTypes { get; set; }
	public DbSet<Floor> Floors { get; set; }
	public DbSet<FinishingType> FinishingTypes { get; set; }

	/// <inheritdoc />
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (optionsBuilder.IsConfigured)
		{
			base.OnConfiguring(optionsBuilder);
			return;
		}

		string workingDirectory = AssemblyHelper.GetEntryAssembly().GetDirectoryPath();
		IHostEnvironment environment = new HostingEnvironment
		{
			EnvironmentName = EnvironmentHelper.GetEnvironmentName(),
			ApplicationName = AppDomain.CurrentDomain.FriendlyName,
			ContentRootPath = workingDirectory,
			ContentRootFileProvider = new PhysicalFileProvider(workingDirectory)
		};

		IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.SetBasePath(workingDirectory);

		IConfiguration configuration = IConfigurationBuilderHelper.CreateConfiguration()
																.AddConfigurationFiles(EnvironmentHelper.GetEnvironmentName())
																.AddEnvironmentVariables()
																.AddUserSecrets()
																.Build();
		DbContextHelper.Setup(optionsBuilder, GetType().Assembly.GetName(), configuration, environment);
		base.OnConfiguring(optionsBuilder);
	}

	/// <inheritdoc />
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.Entity<ApplicationUser>(userIdentity =>
		{
			userIdentity.Property(e => e.Id)
						.HasMaxLength(256);
			userIdentity.Property(e => e.UserName)
						.HasMaxLength(320);
			userIdentity.Property(e => e.NormalizedUserName)
						.HasMaxLength(320);
			userIdentity.Property(e => e.Email)
						.HasMaxLength(320);
			userIdentity.Property(e => e.NormalizedEmail)
						.HasMaxLength(320);

			userIdentity.HasOne<City>()
						.WithMany()
						.HasForeignKey(e => e.CityId)
						.OnDelete(DeleteBehavior.SetNull);

			userIdentity.Property(e => e.CountryCode)
						.HasConversion(e => e, s => s.ToUpper());
			userIdentity.HasOne<Country>()
						.WithMany()
						.HasForeignKey(e => e.CountryCode)
						.OnDelete(DeleteBehavior.Restrict);
		});

		builder.Entity<ApplicationRole>(role =>
		{
			role.Property(e => e.Id)
				.HasMaxLength(256);
			role.Property(e => e.Name)
				.HasMaxLength(256);
			role.Property(e => e.NormalizedName)
				.HasMaxLength(256);
		});
		builder.Entity<Country>(country =>
		{
			country.Property(e => e.Id)
					.HasConversion(e => e, s => s.ToUpper());
		});
		builder.Entity<City>(city =>
		{
			city.Property(e => e.CountryCode)
				.HasConversion(e => e, s => s.ToUpper());
			city.Property(e => e.CountryCode)
				.HasConversion(e => e, s => s.ToUpper());
			city.HasOne<Country>()
				.WithMany()
				.HasForeignKey(e => e.CountryCode)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}

	public async Task<bool> ApplyMigrationsAsync([NotNull] IHost host, bool applySeed = false, ILogger logger = null)
	{
		IServiceScope scope = null;
		logger?.LogInformation("Applying data migrations with data seed.");

		try
		{
			scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
			IServiceProvider serviceProvider = scope.ServiceProvider;
			logger ??= serviceProvider.GetService<ILogger<DataContext>>();
			IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
			logger?.LogInformation("Applying data migrations.");
			await Database.MigrateAsync();
			if (!applySeed) return await IsMigratedAsync();
			await SeedDataAsync(serviceProvider, configuration, logger);
			return true;
		}
		catch (Exception ex)
		{
			logger?.LogError(ex.CollectDataMessages());
			return false;
		}
		finally
		{
			ObjectHelper.Dispose(ref scope);
		}
	}

	public bool IsMigrated()
	{
		return Database.CanConnect() && !Database.GetPendingMigrations().Any();
	}

	public async Task<bool> IsMigratedAsync()
	{
		return await Database.CanConnectAsync() && !(await Database.GetPendingMigrationsAsync()).Any();
	}

	public async Task SeedDataAsync([NotNull] IServiceProvider services, [NotNull] IConfiguration configuration, ILogger logger = null)
	{
		if (!await IsMigratedAsync()) throw new InvalidOperationException("Database is not migrated.");
		logger ??= services.GetService<ILogger<DataContext>>();

		SeedData seedData = configuration.GetSection(nameof(SeedData)).Get<SeedData>();
		if (seedData == null) return;

		IMapper mapper = services.GetRequiredService<IMapper>();

		await SeedCountries(this, logger);

		RoleManager<ApplicationRole> roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
		await SeedRoles(roleManager, seedData, logger);

		if (seedData.Users is { Count: > 0 })
		{
			UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
			await SeedUsers(userManager, seedData.Users, mapper, logger);
		}

		await SeedBuildingMetaData(this, seedData, logger);

		static async Task SeedCountries([NotNull] DataContext context, ILogger logger)
		{
			logger?.LogInformation("Adding countries data.");
			ISet<string> countries = context.Countries
											.Select(e => e.Id)
											.ToHashSet(StringComparer.OrdinalIgnoreCase);
			IEnumerable<Country> newCountries = RegionInfoHelper.Regions.Values
																.OrderBy(e => e.EnglishName)
																.Where(e => !string.IsNullOrEmpty(e.ThreeLetterISORegionName))
																.Select(e => new Country
																{
																	Id = e.ThreeLetterISORegionName,
																	Name = e.EnglishName
																});
			int init = countries.Count;

			foreach (Country country in newCountries)
			{
				if (!countries.Add(country.Id)) continue;
				context.Countries.Add(country);
			}

			await context.SaveChangesAsync();
			logger?.LogInformation($"Added {countries.Count - init} countries.");
		}

		static async Task SeedRoles([NotNull] RoleManager<ApplicationRole> roleManager, [NotNull] SeedData seedData, ILogger logger)
		{
			int rolesAdded = 0;
			logger?.LogInformation("Adding roles data.");

			ISet<string> roles = new HashSet<string>(ApplicationRole.Roles.Keys, StringComparer.OrdinalIgnoreCase);

			if (seedData.Roles is { Count: > 0 })
			{
				foreach (string role in seedData.Roles)
				{
					roles.Add(role);
				}
			}

			foreach (string roleName in roles)
			{
				if (await roleManager.RoleExistsAsync(roleName)) continue;

				IdentityResult result = await roleManager.CreateAsync(new ApplicationRole
				{
					Id = Guid.NewGuid().ToString(),
					Name = roleName
				});

				if (!result.Succeeded)
				{
					throw new DataException(result.Errors.Aggregate(new StringBuilder($"Could not add role '{roleName}'."),
																	(builder, error) => builder.AppendWithLine(error.Description),
																	builder => builder.ToString()));
				}

				logger?.LogInformation($"Added '{roleName}' role.");
				rolesAdded++;
			}

			logger?.LogInformation($"Added {rolesAdded} roles.");
		}

		static async Task SeedUsers(UserManager<ApplicationUser> userManager, [NotNull] IEnumerable<UserData> users, [NotNull] IMapper mapper, ILogger logger)
		{
			logger?.LogInformation("Adding users data.");

			int usersAdded = 0;

			foreach (UserData user in users)
			{
				ApplicationUser userInDb = await userManager.FindByEmailAsync(user.Email);
				IdentityResult result;

				if (userInDb == null)
				{
					userInDb = mapper.Map<ApplicationUser>(user);
					userInDb.Created = DateTime.UtcNow;
					userInDb.Modified = DateTime.UtcNow;
					logger?.LogInformation($"Adding '{userInDb.Email}' user data.");

					result = await userManager.CreateAsync(userInDb, user.Password);

					if (!result.Succeeded)
					{
						logger?.LogError(result.Errors.Aggregate(new StringBuilder($"Could not add user '{user.Email}'."),
																	(builder, error) => builder.AppendWithLine(error.Description),
																	builder => builder.ToString()));
						continue;
					}

					usersAdded++;
					logger?.LogInformation($"Added '{user.Email}'.");
				}

				if (user.Roles is not { Count: > 0 }) continue;
				result = await userManager.AddToRolesAsync(userInDb, user.Roles);

				if (!result.Succeeded)
				{
					logger?.LogWarning(result.Errors.Aggregate(new StringBuilder($"Could not assign user '{user.Email}' to roles."),
																(builder, error) => builder.AppendWithLine(error.Description),
																builder => builder.ToString()));
				}
				else
				{
					logger?.LogInformation($"Added '{user.Email}' to roles {string.Join(", ", user.Roles)}.");
				}
			}

			logger?.LogInformation($"Added {usersAdded} users.");
		}

		static async Task SeedBuildingMetaData([NotNull] DataContext context, [NotNull] SeedData seedData, ILogger logger)
		{
			logger?.LogInformation("Adding building metadata.");

			if (seedData.BuildingTypes is { Count: > 0 } && !await context.BuildingTypes.AnyAsync())
			{
				logger?.LogInformation("Adding building types.");

				foreach (string type in seedData.BuildingTypes)
				{
					context.BuildingTypes.Add(new BuildingType
					{
						Id = type
					});
				}

				await context.SaveChangesAsync();
			}

			if (seedData.FloorTypes is { Count: > 0 } && !await context.Floors.AnyAsync())
			{
				logger?.LogInformation("Adding floor types.");

				foreach (string type in seedData.FloorTypes)
				{
					context.Floors.Add(new Floor
					{
						Id = type
					});
				}

				await context.SaveChangesAsync();
			}

			if (seedData.FinishingTypes is { Count: > 0 } && !await context.FinishingTypes.AnyAsync())
			{
				logger?.LogInformation("Adding finishing types.");

				foreach (string type in seedData.FinishingTypes)
				{
					context.FinishingTypes.Add(new FinishingType
					{
						Id = type
					});
				}

				await context.SaveChangesAsync();
			}

			logger?.LogInformation("Finished adding building metadata.");
		}
	}
}