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

	public DbSet<District> Districts { get; set; }
	public DbSet<City> Cities { get; set; }
	public DbSet<Building> Buildings { get; set; }
	public DbSet<BuildingImage> BuildingImages { get; set; }

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
		builder.Entity<City>(city =>
		{
			city.HasIndex(e => e.Name)
				.IsUnique();
		});
		builder.Entity<District>(district =>
		{
			district.HasOne<City>()
					.WithMany()
					.HasForeignKey(e => e.CityId);
			district.HasIndex(e => e.Name);
		});
		builder.Entity<Building>(building =>
		{
			building.HasOne<City>()
					.WithMany()
					.HasForeignKey(e => e.CityId);

			building.HasIndex(e => e.Reference).IsUnique();
			building.HasIndex(e => e.BuildingType);
			building.HasIndex(e => e.FinishingType).HasFilter(null);
			building.HasIndex(e => e.Floor).HasFilter(null);
			building.HasIndex(e => e.CityId);
			building.HasIndex(e => e.AdType);
			building.HasIndex(e => e.Priority).HasFilter(null);
			building.HasIndex(e => e.Date);
			building.HasIndex(e => e.Expires).HasFilter(null);
			building.HasIndex(e => e.Price);
		});
		builder.Entity<BuildingImage>(image =>
		{
			image.HasOne<Building>()
					.WithMany()
					.HasForeignKey(e => e.BuildingId);
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

		if (seedData.Cities is { Count: > 0 }) await SeedCities(services, seedData.Cities, logger);

		await SeedRoles(services, seedData, logger);

		if (seedData.Users is { Count: > 0 }) await SeedUsers(services, seedData.Users, mapper, logger);

		static async Task SeedCities([NotNull] IServiceProvider services, [NotNull] ICollection<string> cities, ILogger logger)
		{
			logger?.LogInformation("Adding cities data.");

			DataContext context = null;

			try
			{
				context = services.GetRequiredService<DataContext>();

				int addedCities = 0;

				foreach (string city in cities)
				{
					if (context.Cities.FirstOrDefault(e => e.Name == city) != null) continue;
					context.Cities.Add(new City
					{
						Name = city
					});
					addedCities++;
				}

				await context.SaveChangesAsync();
				logger?.LogInformation($"Added {addedCities} of {cities.Count} cities.");
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