﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
	private readonly UserManager _userManager;

	public ConfirmEmailModel([NotNull] UserManager userManager)
	{
		_userManager = userManager;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync(string userId, string code)
	{
		if (userId == null || code == null)
		{
			return RedirectToPage("/Index");
		}

		ApplicationUser user = await _userManager.FindByIdAsync(userId);
		if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

		code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
		StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
		return Page();
	}
}