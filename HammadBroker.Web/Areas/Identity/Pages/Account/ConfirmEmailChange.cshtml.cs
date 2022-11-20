// Licensed to the .NET Foundation under one or more agreements.
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

public class ConfirmEmailChangeModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;

	public ConfirmEmailChangeModel([NotNull] UserManager userManager, [NotNull] SignInManager signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
	{
		if (userId == null || email == null || code == null)
		{
			return RedirectToPage("/Index");
		}

		User user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{userId}'.");
		}

		code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		IdentityResult result = await _userManager.ChangeEmailAsync(user, email, code);
		if (!result.Succeeded)
		{
			StatusMessage = "Error changing email.";
			return Page();
		}

		// In our UI email and user name are one and the same, so when we update the email
		// we need to update the user name.
		IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(user, email);
		if (!setUserNameResult.Succeeded)
		{
			StatusMessage = "Error changing user name.";
			return Page();
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Thank you for confirming your email change.";
		return Page();
	}
}