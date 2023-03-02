using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using essentialMix.Extensions;
using HammadBroker.Model;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HammadBroker.Data.Identity;

public class UserManager : UserManager<User>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private User _systemUser;

    /// <inheritdoc />
    public UserManager([NotNull] IHttpContextAccessor contextAccessor, [NotNull] IUserStore<User> store, [NotNull] IOptions<IdentityOptions> optionsAccessor,
        [NotNull] IPasswordHasher<User> passwordHasher, [NotNull] IEnumerable<IUserValidator<User>> userValidators, [NotNull] IEnumerable<IPasswordValidator<User>> passwordValidators,
        [NotNull] ILookupNormalizer keyNormalizer, [NotNull] IdentityErrorDescriber errors, [NotNull] IServiceProvider services, [NotNull] ILogger<UserManager> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> CreateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (string.IsNullOrEmpty(user.Email)) throw new InvalidOperationException("User email is required.");
        if (string.IsNullOrEmpty(user.UserName)) user.UserName = user.Email;

        User userFromDb = await FindByEmailAsync(user.Email);
        IdentityResult result;

        if (userFromDb == null)
        {
            if (string.IsNullOrEmpty(user.Id)) user.Id = Guid.NewGuid().ToString("D");
            result = await base.CreateAsync(user);
            if (result.Succeeded) await base.AddToRoleAsync(user, Role.Members);
        }
        else
        {
            user.Id = userFromDb.Id;
            result = await UpdateUserAsync(user);
        }

        return result;
    }

    /// <inheritdoc />
    protected override async Task<IdentityResult> UpdateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (!await CanEditUserAsync(user)
            || (await IsDefaultAdminAsync(user.Id) && !user.Email.IsSame(Constants.Authorization.AdministratorId))) return EditFailedForUser(user.UserName);
        return await base.UpdateUserAsync(user);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> SetEmailAsync(User user, string email)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        return !await CanEditUserAsync(user) || (await IsDefaultAdminAsync(user) && !email.IsSame(Constants.Authorization.AdministratorId))
                    ? EditFailedForUser(user.UserName)
                    : await base.SetEmailAsync(user, email);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> ChangeEmailAsync(User user, string newEmail, string token)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        return !await CanEditUserAsync(user) || (await IsDefaultAdminAsync(user) && !newEmail.IsSame(Constants.Authorization.AdministratorId))
                    ? EditFailedForUser(user.UserName)
                    : await base.ChangeEmailAsync(user, newEmail, token);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> SetLockoutEnabledAsync(User user, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        return !await CanEditUserAsync(user) || await IsDefaultAdminAsync(user)
                    ? EditFailedForUser(user.UserName)
                    : await base.SetLockoutEnabledAsync(user, enabled);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> DeleteAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (!await CanEditUserAsync(user) || await IsDefaultAdminAsync(user)) return EditFailedForUser(user.UserName);
        return await base.UpdateUserAsync(user);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (_contextAccessor.HttpContext == null) return await base.AddToRoleAsync(user, role);
        int rank = await GetHighestRankAsync(await GetUserAsync());
        return rank < Role.AdministratorsRank
                || rank < await GetHighestRankAsync(user)
                    ? EditFailedForUser(user.UserName)
                    : await base.AddToRoleAsync(user, role);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (_contextAccessor.HttpContext == null) return await base.AddToRolesAsync(user, roles);
        int rank = await GetHighestRankAsync(await GetUserAsync());
        if (rank < Role.AdministratorsRank
            || rank < await GetHighestRankAsync(user)) return EditFailedForUser(user.UserName);
        return await base.AddToRolesAsync(user, roles);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (_contextAccessor.HttpContext == null) return await base.RemoveFromRoleAsync(user, role);
        int rank = await GetHighestRankAsync(await GetUserAsync());
        if (rank < Role.AdministratorsRank
            || rank < await GetHighestRankAsync(user)
            || (role.IsSame(Role.Administrators) && await IsDefaultAdminAsync(user))) return EditFailedForUser(user.UserName);
        return await base.RemoveFromRoleAsync(user, role);
    }

    /// <inheritdoc />
    public override async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        if (_contextAccessor.HttpContext == null) return await base.RemoveFromRolesAsync(user, roles);
        int rank = await GetHighestRankAsync(await GetUserAsync());
        if (rank < Role.AdministratorsRank
            || rank < await GetHighestRankAsync(user)) return EditFailedForUser(user.UserName);
        if (await IsDefaultAdminAsync(user)) roles = roles.Where(e => !e.IsSame(Role.Administrators));
        return await base.RemoveFromRolesAsync(user, roles);
    }

    private async Task<User> GetAdminUserAsync()
    {
        _systemUser ??= await FindByEmailAsync(Constants.Authorization.AdministratorId);
        return _systemUser;
    }

    private async Task<User> GetUserAsync()
    {
        string userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == null
                    ? null
                    : await FindByIdAsync(userId);
    }

    private async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return user == null
                    ? null
                    : await GetRolesAsync(user);
    }

    private async Task<bool> IsDefaultAdminAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || _contextAccessor.HttpContext == null) return false;
        User systemUser = await GetAdminUserAsync();
        return systemUser != null && systemUser.Id.IsSame(userId);
    }

    private async Task<bool> IsDefaultAdminAsync(User user)
    {
        if (user == null || _contextAccessor.HttpContext == null || !await IsInRoleAsync(user, Role.Administrators)) return false;
        User systemUser = await GetAdminUserAsync();
        return systemUser != null && (systemUser == user || systemUser.Email.IsSame(user.Email) || systemUser.UserName.IsSame(user.UserName));
    }

    private async Task<int> GetHighestRankAsync(User user)
    {
        if (user == null) return -1;

        try
        {
            IList<string> roles = await GetUserRolesAsync(user);
            if (roles == null || roles.Count == 0) return -1;
            return Role.GetHighestRank(roles);
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private async Task<bool> CanEditUserAsync(User user)
    {
        if (user == null) return false;
        if (_contextAccessor.HttpContext == null) return true;
        User currentUser = await GetUserAsync();
        return currentUser == null || await CanEditUserAsync(currentUser, user);
    }

    private async Task<bool> CanEditUserAsync(User x, User y)
    {
        if (ReferenceEquals(y, null)) return false;
        if (ReferenceEquals(x, null) || ReferenceEquals(x, y)) return true;
        int rx = await GetHighestRankAsync(x);
        return rx >= Role.AdministratorsRank && rx > await GetHighestRankAsync(y);
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