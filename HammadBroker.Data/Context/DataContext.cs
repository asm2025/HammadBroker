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
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
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

public class DataContext : IdentityDbContext<User, Role, string,
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
	public DbSet<Building> Buildings { get; set; }
	public DbSet<BuildingImage> BuildingImages { get; set; }
	public DbSet<BuildingAd> BuildingAds { get; set; }

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
		builder.Entity<User>(userIdentity =>
		{
			userIdentity.ToTable("Users");
			userIdentity.Property(e => e.UserName)
						.HasMaxLength(320);
			userIdentity.Property(e => e.NormalizedUserName)
						.HasMaxLength(320);
			userIdentity.Property(e => e.Email)
						.HasMaxLength(320);
			userIdentity.Property(e => e.NormalizedEmail)
						.HasMaxLength(320);
		});
		builder.Entity<Role>(role =>
		{
			role.ToTable("Roles");
			role.Property(e => e.Name)
				.HasMaxLength(256);
			role.Property(e => e.NormalizedName)
				.HasMaxLength(256);
		});
		builder.Entity<IdentityUserRole<string>>(entity =>
		{
			entity.ToTable("UserRoles");
		});
		builder.Entity<IdentityUserClaim<string>>(entity =>
		{
			entity.ToTable("UserClaims");
		});
		builder.Entity<IdentityUserLogin<string>>(entity =>
		{
			entity.ToTable("UserLogins");
		});
		builder.Entity<IdentityRoleClaim<string>>(entity =>
		{
			entity.ToTable("RoleClaims");
		});
		builder.Entity<IdentityUserToken<string>>(entity =>
		{
			entity.ToTable("UserTokens");
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
			city.HasOne<Country>()
				.WithMany()
				.HasForeignKey(e => e.CountryCode);
			city.HasIndex(e => new
			{
				e.CountryCode,
				e.Name
			})
			.IsUnique();
		});
		builder.Entity<Building>(building =>
		{
			building.HasOne<City>()
					.WithMany()
					.HasForeignKey(e => e.CityId);

			building.HasIndex(e => e.BuildingType);
			building.HasIndex(e => e.FinishingType).HasFilter(null);
			building.HasIndex(e => e.Floor).HasFilter(null);
			building.HasIndex(e => e.Rooms).HasFilter(null);
			building.HasIndex(e => e.Bathrooms).HasFilter(null);
			building.HasIndex(e => e.Area).HasFilter(null);
		});
		builder.Entity<BuildingImage>(image =>
		{
			image.HasOne<Building>()
					.WithMany()
					.HasForeignKey(e => e.BuildingId);
		});
		builder.Entity<BuildingAd>(ad =>
		{
			ad.HasOne<Building>()
			  .WithMany()
			  .HasForeignKey(e => e.BuildingId);
			ad.HasIndex(e => e.Type);
			ad.HasIndex(e => e.Priority);
			ad.HasIndex(e => e.Date);
			ad.HasIndex(e => e.Expires).HasFilter(null);
			ad.HasIndex(e => e.Price);
		});
	}

	public async Task<bool> ApplyMigrationsAsync([NotNull] IHost host, [NotNull] IConfiguration configuration, bool applySeed = false, ILogger logger = null)
	{
		IServiceScope scope = null;

		try
		{
			scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
			IServiceProvider serviceProvider = scope.ServiceProvider;
			logger ??= serviceProvider.GetService<ILogger<DataContext>>();
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

		ISet<string> countries = await SeedCountries(services, logger);

		if (seedData.Cities is { Count: > 0 }) await SeedCities(services, seedData.Cities, countries, logger);

		await SeedRoles(services, seedData, logger);

		if (seedData.Users is { Count: > 0 }) await SeedUsers(services, seedData.Users, mapper, logger);

		[ItemNotNull]
		static async Task<ISet<string>> SeedCountries([NotNull] IServiceProvider services, ILogger logger)
		{
			logger?.LogInformation("Adding countries data.");

			DataContext context = null;
			ISet<string> countries;

			try
			{
				context = services.GetRequiredService<DataContext>();
				countries = context.Countries
									.Select(e => e.Id)
									.ToHashSet(StringComparer.OrdinalIgnoreCase);
				IEnumerable<Country> newCountries = RegionInfoHelper.Regions.Values
																	.OrderBy(e => e.EnglishName)
																	.Where(e => !string.IsNullOrEmpty(e.ThreeLetterISORegionName))
																	.Select(e => new Country
																	{
																		Id = e.ThreeLetterISORegionName,
																		Name = e.NativeName
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
			finally
			{
				ObjectHelper.Dispose(ref context);
			}

			return countries;
		}

		static async Task SeedCities([NotNull] IServiceProvider services, [NotNull] ICollection<CitiesData> citiesData, ISet<string> countries, ILogger logger)
		{
			logger?.LogInformation("Adding cities data.");

			DataContext context = null;

			try
			{
				context = services.GetRequiredService<DataContext>();

				int addedCountries = 0;

				foreach (CitiesData data in citiesData)
				{
					if (!countries.Contains(data.CountryCode))
					{
						logger?.LogWarning($"Country '{data.CountryCode}' was not found while seeding cities.");
						continue;
					}

					int addedCities = 0;

					foreach (string city in data.Cities)
					{
						if (context.Cities.FirstOrDefault(e => e.CountryCode == data.CountryCode && e.Name == city) != null) continue;
						context.Cities.Add(new City
						{
							CountryCode = data.CountryCode,
							Name = city
						});
						addedCities++;
					}

					await context.SaveChangesAsync();
					addedCountries++;
					logger?.LogInformation($"Added {addedCities} of {citiesData.Count} cities to {data.CountryCode} country.");
				}

				logger?.LogInformation($"Added cities to {addedCountries} of {citiesData.Count} countries.");
			}
			finally
			{
				ObjectHelper.Dispose(ref context);
			}
		}

		static async Task SeedRoles([NotNull] IServiceProvider services, [NotNull] SeedData seedData, ILogger logger)
		{
			int rolesAdded = 0;
			logger?.LogInformation("Adding roles data.");

			RoleManager<Role> roleManager = null;

			try
			{
				roleManager = services.GetRequiredService<RoleManager<Role>>();

				ISet<string> roles = new HashSet<string>(Role.Roles.Keys, StringComparer.OrdinalIgnoreCase);

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

					IdentityResult result = await roleManager.CreateAsync(new Role
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
			finally
			{
				ObjectHelper.Dispose(ref roleManager);
			}
		}

		static async Task SeedUsers([NotNull] IServiceProvider services, [NotNull] IEnumerable<UserData> users, [NotNull] IMapper mapper, ILogger logger)
		{
			logger?.LogInformation("Adding users data.");

			int usersAdded = 0;
			UserManager<User> userManager = null;

			try
			{
				userManager = services.GetRequiredService<UserManager<User>>();

				foreach (UserData user in users)
				{
					User userInDb = await userManager.FindByEmailAsync(user.Email);
					IdentityResult result;

					if (userInDb == null)
					{
						userInDb = mapper.Map<User>(user);
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
			}
			finally
			{
				ObjectHelper.Dispose(ref userManager);
			}

			logger?.LogInformation($"Added {usersAdded} users.");
		}
	}
}