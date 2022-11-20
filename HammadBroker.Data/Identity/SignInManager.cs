using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HammadBroker.Data.Identity;

public class SignInManager : SignInManager<User>
{
	/// <inheritdoc />
	public SignInManager([NotNull] UserManager<User> userManager, [NotNull] IHttpContextAccessor contextAccessor, [NotNull] IUserClaimsPrincipalFactory<User> claimsFactory,
		[NotNull] IOptions<IdentityOptions> optionsAccessor, [NotNull] ILogger<SignInManager<User>> logger, [NotNull] IAuthenticationSchemeProvider schemes, [NotNull] IUserConfirmation<User> confirmation)
		: base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
	{
	}
}