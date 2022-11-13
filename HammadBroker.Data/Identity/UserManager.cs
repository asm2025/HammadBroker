using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using essentialMix.Extensions;
using HammadBroker.Model;
using HammadBroker.Model.Entities;
using IdentityModel;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HammadBroker.Data.Identity;

public class UserManager : UserManager<ApplicationUser>
{
	private readonly IHttpContextAccessor _contextAccessor;
	private ApplicationUser _systemUser;

	/// <inheritdoc />
	public UserManager([NotNull] IHttpContextAccessor contextAccessor, [NotNull] IUserStore<ApplicationUser> store, [NotNull] IOptions<IdentityOptions> optionsAccessor,
		[NotNull] IPasswordHasher<ApplicationUser> passwordHasher, [NotNull] IEnumerable<IUserValidator<ApplicationUser>> userValidators, [NotNull] IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
		[NotNull] ILookupNormalizer keyNormalizer, [NotNull] IdentityErrorDescriber errors, [NotNull] IServiceProvider services, [NotNull] ILogger<UserManager> logger)
		: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
	{
		_contextAccessor = contextAccessor;
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> CreateAsync([NotNull] ApplicationUser user)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (string.IsNullOrEmpty(user.Email)) throw new InvalidOperationException("User email is required.");

		ApplicationUser userFromDb = await FindByEmailAsync(user.Email);
		IdentityResult result;

		if (userFromDb == null)
		{
			result = await base.CreateAsync(user);
			if (result.Succeeded) await base.AddToRoleAsync(user, ApplicationRole.Members);
		}
		else
		{
			user.Id = userFromDb.Id;
			result = await UpdateUserAsync(user);
		}

		return result;
	}

	/// <inheritdoc />
	[ItemNotNull]
	protected override async Task<IdentityResult> UpdateUserAsync([NotNull] ApplicationUser user)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (!await CanEditUserAsync(user)
			|| (await IsDefaultSuperUserAsync(user.Id) && !user.Email.IsSame(Constants.Authorization.AdministratorId))) return EditFailedForUser(user.UserName);
		return await base.UpdateUserAsync(user);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> SetEmailAsync([NotNull] ApplicationUser user, string email)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		return !await CanEditUserAsync(user) || (await IsDefaultSuperUserAsync(user) && !email.IsSame(Constants.Authorization.AdministratorId))
					? EditFailedForUser(user.UserName)
					: await base.SetEmailAsync(user, email);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> ChangeEmailAsync([NotNull] ApplicationUser user, string newEmail, string token)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		return !await CanEditUserAsync(user) || (await IsDefaultSuperUserAsync(user) && !newEmail.IsSame(Constants.Authorization.AdministratorId))
					? EditFailedForUser(user.UserName)
					: await base.ChangeEmailAsync(user, newEmail, token);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> SetLockoutEnabledAsync([NotNull] ApplicationUser user, bool enabled)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		return !await CanEditUserAsync(user) || await IsDefaultSuperUserAsync(user)
					? EditFailedForUser(user.UserName)
					: await base.SetLockoutEnabledAsync(user, enabled);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> DeleteAsync([NotNull] ApplicationUser user)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (!await CanEditUserAsync(user) || await IsDefaultSuperUserAsync(user)) return EditFailedForUser(user.UserName);
		return await base.UpdateUserAsync(user);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> AddToRoleAsync([NotNull] ApplicationUser user, string role)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (_contextAccessor.HttpContext == null) return await base.AddToRoleAsync(user, role);
		int rank = await GetHighestRankAsync(await GetUserAsync());
		return rank < ApplicationRole.AdministratorsRank
				|| rank < ApplicationRole.GetRank(role)
				|| rank < await GetHighestRankAsync(user)
					? EditFailedForUser(user.UserName)
					: await base.AddToRoleAsync(user, role);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> AddToRolesAsync([NotNull] ApplicationUser user, IEnumerable<string> roles)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (_contextAccessor.HttpContext == null) return await base.AddToRolesAsync(user, roles);
		int rank = await GetHighestRankAsync(await GetUserAsync());
		if (rank < ApplicationRole.AdministratorsRank
			|| rank < ApplicationRole.GetHighestRank(roles)
			|| rank < await GetHighestRankAsync(user)) return EditFailedForUser(user.UserName);
		return await base.AddToRolesAsync(user, roles);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> RemoveFromRoleAsync([NotNull] ApplicationUser user, string role)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (_contextAccessor.HttpContext == null) return await base.RemoveFromRoleAsync(user, role);
		int rank = await GetHighestRankAsync(await GetUserAsync());
		if (rank < ApplicationRole.AdministratorsRank
			|| rank < await GetHighestRankAsync(user)
			|| (role.IsSame(ApplicationRole.System) && await IsDefaultSuperUserAsync(user))) return EditFailedForUser(user.UserName);
		return await base.RemoveFromRoleAsync(user, role);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public override async Task<IdentityResult> RemoveFromRolesAsync([NotNull] ApplicationUser user, IEnumerable<string> roles)
	{
		ArgumentNullException.ThrowIfNull(user, nameof(user));
		if (_contextAccessor.HttpContext == null) return await base.RemoveFromRolesAsync(user, roles);
		int rank = await GetHighestRankAsync(await GetUserAsync());
		if (rank < ApplicationRole.AdministratorsRank
			|| rank < ApplicationRole.GetHighestRank(roles)
			|| rank < await GetHighestRankAsync(user)) return EditFailedForUser(user.UserName);
		if (await IsDefaultSuperUserAsync(user)) roles = roles.Where(e => !e.IsSame(ApplicationRole.System));
		return await base.RemoveFromRolesAsync(user, roles);
	}

	private async Task<ApplicationUser> GetSystemUserAsync()
	{
		_systemUser ??= await FindByEmailAsync(Constants.Authorization.AdministratorId);
		return _systemUser;
	}

	private async Task<ApplicationUser> GetUserAsync()
	{
		string userId = _contextAccessor.HttpContext?.User.FindFirstValue(JwtClaimTypes.Subject);
		return userId == null
					? null
					: await FindByIdAsync(userId);
	}

	private async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
	{
		return user == null
					? null
					: await GetRolesAsync(user);
	}

	private async Task<bool> IsDefaultSuperUserAsync(string userId)
	{
		if (string.IsNullOrEmpty(userId) || _contextAccessor.HttpContext == null) return false;
		ApplicationUser systemUser = await GetSystemUserAsync();
		return systemUser != null && systemUser.Id.IsSame(userId);
	}

	private async Task<bool> IsDefaultSuperUserAsync(ApplicationUser user)
	{
		if (user == null || _contextAccessor.HttpContext == null || !await IsInRoleAsync(user, ApplicationRole.System)) return false;
		ApplicationUser systemUser = await GetSystemUserAsync();
		return systemUser != null && (systemUser == user || systemUser.Email.IsSame(user.Email) || systemUser.UserName.IsSame(user.UserName));
	}

	private async Task<int> GetHighestRankAsync(ApplicationUser user)
	{
		if (user == null) return -1;

		try
		{
			IList<string> roles = await GetUserRolesAsync(user);
			if (roles == null || roles.Count == 0) return -1;
			return ApplicationRole.GetHighestRank(roles);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	private async Task<bool> CanEditUserAsync(ApplicationUser user)
	{
		if (user == null) return false;
		if (_contextAccessor.HttpContext == null) return true;
		ApplicationUser currentUser = await GetUserAsync();
		return currentUser == null || await CanEditUserAsync(currentUser, user);
	}

	private async Task<bool> CanEditUserAsync(ApplicationUser x, ApplicationUser y)
	{
		if (ReferenceEquals(y, null)) return false;
		if (ReferenceEquals(x, null) || ReferenceEquals(x, y)) return true;
		int rx = await GetHighestRankAsync(x);
		return rx >= ApplicationRole.AdministratorsRank && rx > await GetHighestRankAsync(y);
	}

	[NotNull]
	private static IdentityResult EditFailedForUser(string userName = null)
	{
		userName = userName.ToNullIfEmpty().Suffix(" user") ?? "user";
		return IdentityResult.Failed(new IdentityError
		{
			Code = "Forbidden",
			Description = $"Cannot edit {userName}."
		});
	}
}