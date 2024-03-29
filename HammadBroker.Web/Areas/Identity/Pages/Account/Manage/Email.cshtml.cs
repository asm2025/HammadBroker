﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly IEmailSender _emailSender;

	public EmailModel(
		[NotNull] UserManager userManager,
		[NotNull] SignInManager signInManager,
		[NotNull] IEmailSender emailSender)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_emailSender = emailSender;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[Display(Name = "البريد الالكتروني")]
	public string Email { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[Display(Name = "البريد الالكتروني مؤكد")]
	public bool IsEmailConfirmed { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string StatusMessage { get; set; }

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
		[EmailAddress]
		[Display(Name = "البريد الالكتروني الجديد")]
		public string NewEmail { get; set; }
	}

	private async Task LoadAsync(User user)
	{
		string email = await _userManager.GetEmailAsync(user);
		Email = email;

		Input = new InputModel
		{
			NewEmail = email,
		};

		IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
	}

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		await LoadAsync(user);
		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostChangeEmailAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if (!ModelState.IsValid)
		{
			await LoadAsync(user);
			return Page();
		}

		string email = await _userManager.GetEmailAsync(user);
		if (Input.NewEmail != email)
		{
			string userId = await _userManager.GetUserIdAsync(user);
			string code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			string callbackUrl = Url.Page(
										"/Account/ConfirmEmailChange",
										null,
										new
										{
											area = "Identity",
											userId,
											email = Input.NewEmail,
											code
										},
										Request.Scheme);
			await _emailSender.SendEmailAsync(
											Input.NewEmail,
											"Confirm your email",
											$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

			StatusMessage = "Confirmation link to change email sent. Please check your email.";
			return RedirectToPage();
		}

		StatusMessage = "Your email is unchanged.";
		return RedirectToPage();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostSendVerificationEmailAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if (!ModelState.IsValid)
		{
			await LoadAsync(user);
			return Page();
		}

		string userId = await _userManager.GetUserIdAsync(user);
		string email = await _userManager.GetEmailAsync(user);
		string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		string callbackUrl = Url.Page(
									"/Account/ConfirmEmail",
									null,
									new
									{
										area = "Identity",
										userId,
										code
									},
									Request.Scheme);
		await _emailSender.SendEmailAsync(
										email,
										"Confirm your email",
										$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

		StatusMessage = "Verification email sent. Please check your email.";
		return RedirectToPage();
	}
}