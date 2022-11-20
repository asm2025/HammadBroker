﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class DeletePersonalDataModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly ILogger _logger;

	public DeletePersonalDataModel(
		[NotNull] UserManager userManager,
		[NotNull] SignInManager signInManager,
		[NotNull] ILogger<DeletePersonalDataModel> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
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
	public class InputModel
	{
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool RequirePassword { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGet()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		RequirePassword = await _userManager.HasPasswordAsync(user);
		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		RequirePassword = await _userManager.HasPasswordAsync(user);
		if (RequirePassword)
		{
			if (!await _userManager.CheckPasswordAsync(user, Input.Password))
			{
				ModelState.AddModelError(string.Empty, "Incorrect password.");
				return Page();
			}
		}

		IdentityResult result = await _userManager.DeleteAsync(user);
		string userId = await _userManager.GetUserIdAsync(user);
		if (!result.Succeeded)
		{
			throw new InvalidOperationException($"Unexpected error occurred deleting user.");
		}

		await _signInManager.SignOutAsync();

		_logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

		return Redirect("~/");
	}
}