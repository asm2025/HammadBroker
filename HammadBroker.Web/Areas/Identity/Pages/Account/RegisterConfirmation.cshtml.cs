// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly IEmailSender _sender;

	public RegisterConfirmationModel([NotNull] UserManager userManager, [NotNull] IEmailSender sender)
	{
		_userManager = userManager;
		_sender = sender;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool DisplayConfirmAccountLink { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string EmailConfirmationUrl { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
	{
		if (email == null)
		{
			return RedirectToPage("/Index");
		}
		returnUrl = returnUrl ?? Url.Content("~/");

		ApplicationUser user = await _userManager.FindByEmailAsync(email);
		if (user == null)
		{
			return NotFound($"Unable to load user with email '{email}'.");
		}

		Email = email;
		// Once you add a real email sender, you should remove this code that lets you confirm the account
		DisplayConfirmAccountLink = true;
		if (DisplayConfirmAccountLink)
		{
			string userId = await _userManager.GetUserIdAsync(user);
			string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			EmailConfirmationUrl = Url.Page(
											"/Account/ConfirmEmail",
											null,
											new
											{
												area = "Identity",
												userId,
												code,
												returnUrl
											},
											Request.Scheme);
		}

		return Page();
	}
}