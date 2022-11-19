﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
	private readonly SignInManager _signInManager;
	private readonly UserManager _userManager;
	private readonly ILogger _logger;

	public LoginWithRecoveryCodeModel([NotNull] SignInManager signInManager, [NotNull] UserManager userManager, [NotNull] ILogger<LoginWithRecoveryCodeModel> logger)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty]
	public InputModel Input { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public class InputModel
	{
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		[Required]
		[DataType(DataType.Text)]
		[Display(Name = "Recovery Code")]
		public string RecoveryCode { get; set; }
	}

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync(string returnUrl = null)
	{
		// Ensure the user has gone through the username & password screen first
		ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user == null)
		{
			throw new InvalidOperationException($"Unable to load two-factor authentication user.");
		}

		ReturnUrl = returnUrl;

		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync(string returnUrl = null)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user == null)
		{
			throw new InvalidOperationException($"Unable to load two-factor authentication user.");
		}

		string recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

		SignInResult result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

		string userId = await _userManager.GetUserIdAsync(user);

		if (result.Succeeded)
		{
			_logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
			return LocalRedirect(returnUrl ?? Url.Content("~/"));
		}
		if (result.IsLockedOut)
		{
			_logger.LogWarning("User account locked out.");
			return RedirectToPage("./Lockout");
		}

		_logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
		ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
		return Page();
	}
}