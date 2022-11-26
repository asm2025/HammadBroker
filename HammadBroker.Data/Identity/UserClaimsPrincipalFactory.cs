using System.Security.Claims;
using System.Threading.Tasks;
using essentialMix.Extensions;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HammadBroker.Data.Identity;

public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
{
	/// <inheritdoc />
	public UserClaimsPrincipalFactory([NotNull] UserManager<User> userManager, [NotNull] IOptions<IdentityOptions> optionsAccessor)
		: base(userManager, optionsAccessor)
	{
	}

	/// <inheritdoc />
	[ItemNotNull]
	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
	{
		ClaimsIdentity claims = await base.GenerateClaimsAsync(user);
		string name = user.NickName.ToNullIfEmpty() ?? user.FirstName;

		if (!string.IsNullOrEmpty(name))
		{
			Claim claim = claims.FindFirst(ClaimTypes.Name);
			if (claim != null) claims.RemoveClaim(claim);
			claims.AddClaim(new Claim(ClaimTypes.Name, name));
		}

		if (!string.IsNullOrEmpty(user.FirstName)) claims.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
		if (!string.IsNullOrEmpty(user.LastName)) claims.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
		return claims;
	}
}