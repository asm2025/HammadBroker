// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class ResetAuthenticatorModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly ILogger _logger;

	public ResetAuthenticatorModel(
		[NotNull] UserManager userManager,
		[NotNull] SignInManager signInManager,
		[NotNull] ILogger<ResetAuthenticatorModel> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGet()
	{
		ApplicationUser user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync()
	{
		ApplicationUser user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		await _userManager.SetTwoFactorEnabledAsync(user, false);
		await _userManager.ResetAuthenticatorKeyAsync(user);
		string userId = await _userManager.GetUserIdAsync(user);
		_logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

		return RedirectToPage("./EnableAuthenticator");
	}
}