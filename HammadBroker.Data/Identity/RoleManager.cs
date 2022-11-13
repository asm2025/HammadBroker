using System.Collections.Generic;
using System.Threading.Tasks;
using essentialMix.Extensions;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Identity;

public class RoleManager : RoleManager<ApplicationRole>
{
	/// <inheritdoc />
	public RoleManager([NotNull] IRoleStore<ApplicationRole> store, [NotNull] IEnumerable<IRoleValidator<ApplicationRole>> roleValidators,
		[NotNull] ILookupNormalizer keyNormalizer, [NotNull] IdentityErrorDescriber errors, [NotNull] ILogger<RoleManager> logger)
		: base(store, roleValidators, keyNormalizer, errors, logger)
	{
	}

	/// <inheritdoc />
	[NotNull]
	public override Task<IdentityResult> SetRoleNameAsync(ApplicationRole role, string name)
	{
		if ((IsSystemRole(role) && !IsSystemRole(name))
			|| (IsAdminRole(role) && !IsAdminRole(name))) return Task.FromResult(FailedAdminRole());
		return base.SetRoleNameAsync(role, name);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> UpdateAsync([NotNull] ApplicationRole role)
	{
		ApplicationRole roleFromDb = await FindByIdAsync(role.Id);
		if ((IsSystemRole(roleFromDb) && !IsSystemRole(ApplicationRole.System))
			|| (IsAdminRole(roleFromDb) && !IsAdminRole(ApplicationRole.Administrators))) return FailedAdminRole();
		return await base.UpdateAsync(role);
	}

	/// <inheritdoc />
	[ItemNotNull]
	protected override async Task<IdentityResult> UpdateRoleAsync([NotNull] ApplicationRole role)
	{
		ApplicationRole roleFromDb = await FindByIdAsync(role.Id);
		if (roleFromDb != null
			&& ((IsSystemRole(roleFromDb) && !IsSystemRole(role.Name))
			|| (IsAdminRole(roleFromDb) && !IsAdminRole(role.Name)))) return FailedAdminRole();
		return await base.UpdateRoleAsync(role);
	}

	/// <inheritdoc />
	[NotNull]
	public override Task<IdentityResult> DeleteAsync([NotNull] ApplicationRole role)
	{
		return ApplicationRole.Roles.ContainsKey(role.Name)
					? Task.FromResult(FailedAdminRole())
					: base.DeleteAsync(role);
	}

	private static bool IsSystemRole(ApplicationRole role)
	{
		return role != null && role.Name.IsSame(ApplicationRole.System);
	}

	private static bool IsSystemRole(string role)
	{
		return role.IsSame(ApplicationRole.System);
	}

	private static bool IsAdminRole(ApplicationRole role)
	{
		return role != null && role.Name.IsSame(ApplicationRole.Administrators);
	}

	private static bool IsAdminRole(string role)
	{
		return role.IsSame(ApplicationRole.Administrators);
	}

	[NotNull]
	private static IdentityResult FailedAdminRole()
	{
		return IdentityResult.Failed(new IdentityError
		{
			Code = "Forbidden",
			Description = "Cannot edit role."
		});
	}
}