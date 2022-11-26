﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class ExternalLoginsModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly IUserStore<User> _userStore;

	public ExternalLoginsModel(
		[NotNull] UserManager userManager,
		[NotNull] SignInManager signInManager,
		[NotNull] IUserStore<User> userStore)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_userStore = userStore;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public IList<UserLoginInfo> CurrentLogins { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public IList<AuthenticationScheme> OtherLogins { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool ShowRemoveButton { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		CurrentLogins = await _userManager.GetLoginsAsync(user);
		OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
					.Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
					.ToList();

		string passwordHash = null;
		if (_userStore is IUserPasswordStore<User> userPasswordStore)
		{
			passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
		}

		ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;
		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		IdentityResult result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
		if (!result.Succeeded)
		{
			StatusMessage = "The external login was not removed.";
			return RedirectToPage();
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "The external login was removed.";
		return RedirectToPage();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostLinkLoginAsync([NotNull] string provider)
	{
		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

		// Request a redirect to the external login provider to link a login for the current user
		string redirectUrl = Url.Page("./ExternalLogins", "LinkLoginCallback");
		AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
		return new ChallengeResult(provider, properties);
	}

	[ItemNotNull]
	public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		string userId = await _userManager.GetUserIdAsync(user);
		ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync(userId);
		if (info == null)
		{
			throw new InvalidOperationException("Unexpected error occurred loading external login info.");
		}

		IdentityResult result = await _userManager.AddLoginAsync(user, info);
		if (!result.Succeeded)
		{
			StatusMessage = "The external login was not added. External logins can only be associated with one account.";
			return RedirectToPage();
		}

		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

		StatusMessage = "The external login was added.";
		return RedirectToPage();
	}
}