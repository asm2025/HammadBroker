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

public class TwoFactorAuthenticationModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly ILogger _logger;

	public TwoFactorAuthenticationModel(
		UserManager userManager, SignInManager signInManager, ILogger<TwoFactorAuthenticationModel> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool HasAuthenticator { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public int RecoveryCodesLeft { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty]
	public bool Is2faEnabled { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool IsMachineRemembered { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync()
	{
		ApplicationUser user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
		Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
		IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
		RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

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

		await _signInManager.ForgetTwoFactorClientAsync();
		StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
		return RedirectToPage();
	}
}