using System.Collections.Generic;
using System.Threading.Tasks;
using essentialMix.Extensions;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Identity;

public class RoleManager : RoleManager<Role>
{
	/// <inheritdoc />
	public RoleManager([NotNull] IRoleStore<Role> store, [NotNull] IEnumerable<IRoleValidator<Role>> roleValidators,
		[NotNull] ILookupNormalizer keyNormalizer, [NotNull] IdentityErrorDescriber errors, [NotNull] ILogger<RoleManager> logger)
		: base(store, roleValidators, keyNormalizer, errors, logger)
	{
	}

	/// <inheritdoc />
	public override Task<IdentityResult> SetRoleNameAsync(Role role, string name)
	{
		if ((IsSystemRole(role) && !IsSystemRole(name))
			|| (IsAdminRole(role) && !IsAdminRole(name))) return Task.FromResult(FailedAdminRole());
		return base.SetRoleNameAsync(role, name);
	}

	/// <inheritdoc />
	public override async Task<IdentityResult> UpdateAsync(Role role)
	{
		Role roleFromDb = await FindByIdAsync(role.Id);
		if ((IsSystemRole(roleFromDb) && !IsSystemRole(Role.System))
			|| (IsAdminRole(roleFromDb) && !IsAdminRole(Role.Administrators))) return FailedAdminRole();
		return await base.UpdateAsync(role);
	}

	/// <inheritdoc />
	protected override async Task<IdentityResult> UpdateRoleAsync(Role role)
	{
		Role roleFromDb = await FindByIdAsync(role.Id);
		if (roleFromDb != null
			&& ((IsSystemRole(roleFromDb) && !IsSystemRole(role.Name))
			|| (IsAdminRole(roleFromDb) && !IsAdminRole(role.Name)))) return FailedAdminRole();
		return await base.UpdateRoleAsync(role);
	}

	/// <inheritdoc />
	public override Task<IdentityResult> DeleteAsync(Role role)
	{
		return Role.Roles.ContainsKey(role.Name)
					? Task.FromResult(FailedAdminRole())
					: base.DeleteAsync(role);
	}

	private static bool IsSystemRole(Role role)
	{
		return role != null && role.Name.IsSame(Role.System);
	}

	private static bool IsSystemRole(string role)
	{
		return role.IsSame(Role.System);
	}

	private static bool IsAdminRole(Role role)
	{
		return role != null && role.Name.IsSame(Role.Administrators);
	}

	private static bool IsAdminRole(string role)
	{
		return role.IsSame(Role.Administrators);
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