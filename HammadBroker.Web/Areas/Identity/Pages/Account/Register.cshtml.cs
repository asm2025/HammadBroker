// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HammadBroker.Data.Identity;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

[Authorize(Policy = Role.System)]
public class RegisterModel : PageModel
{
	private readonly SignInManager _signInManager;
	private readonly UserManager _userManager;
	private readonly IUserStore<User> _userStore;
	private readonly IUserEmailStore<User> _emailStore;
	private readonly IMapper _mapper;
	private readonly ILogger _logger;
	private readonly IEmailSender _emailSender;

	public RegisterModel(
		[NotNull] UserManager userManager,
		[NotNull] IUserStore<User> userStore,
		[NotNull] SignInManager signInManager,
		[NotNull] IMapper mapper,
		[NotNull] ILogger<RegisterModel> logger,
		[NotNull] IEmailSender emailSender)
	{
		_userManager = userManager;
		_userStore = userStore;
		_emailStore = GetEmailStore();
		_signInManager = signInManager;
		_mapper = mapper;
		_logger = logger;
		_emailSender = emailSender;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty(Name = "")]
	public UserToRegister Input { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	public async Task OnGetAsync(string returnUrl = null)
	{
		ReturnUrl = returnUrl;
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync(string returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		if (!ModelState.IsValid) return Page();

		User user = _mapper.Map<User>(Input);
		await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
		await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

		IdentityResult result = await _userManager.CreateAsync(user, Input.Password);

		if (result.Succeeded)
		{
			_logger.LogInformation("User created a new account with password.");

			string userId = await _userManager.GetUserIdAsync(user);
			string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			string callbackUrl = Url.Page(
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

			await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
											  $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

			if (_userManager.Options.SignIn.RequireConfirmedAccount)
			{
				return RedirectToPage("RegisterConfirmation", new
				{
					email = Input.Email,
					returnUrl
				});
			}

			await _signInManager.SignInAsync(user, false);
			return LocalRedirect(returnUrl);
		}
		foreach (IdentityError error in result.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		// If we got this far, something failed, redisplay form
		return Page();
	}

	private IUserEmailStore<User> GetEmailStore()
	{
		if (!_userManager.SupportsUserEmail)
		{
			throw new NotSupportedException("The default UI requires a user store with email support.");
		}
		return (IUserEmailStore<User>)_userStore;
	}
}