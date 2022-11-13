using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class IdentityRepository : Repository<DataContext, ApplicationUser, string>, IIdentityRepository
{
	/// <inheritdoc />
	public IdentityRepository([NotNull] DataContext context, [NotNull] UserManager userManager, [NotNull] RoleManager roleManager, [NotNull] IConfiguration configuration, [NotNull] ILogger<IdentityRepository> logger)
		: base(context, configuration, logger)
	{
		UserManager = userManager;
		RoleManager = roleManager;
	}

	public UserManager UserManager { get; }

	public RoleManager RoleManager { get; }
}