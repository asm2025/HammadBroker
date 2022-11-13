using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HammadBroker.Data.Identity;

public class SignInManager : SignInManager<ApplicationUser>
{
	/// <inheritdoc />
	public SignInManager([NotNull] UserManager<ApplicationUser> userManager, [NotNull] IHttpContextAccessor contextAccessor, [NotNull] IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
		[NotNull] IOptions<IdentityOptions> optionsAccessor, [NotNull] ILogger<SignInManager<ApplicationUser>> logger, [NotNull] IAuthenticationSchemeProvider schemes, [NotNull] IUserConfirmation<ApplicationUser> confirmation)
		: base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
	{
	}
}