// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
	private readonly SignInManager _signInManager;
	private readonly UserManager _userManager;
	private readonly IUserStore<User> _userStore;
	private readonly IUserEmailStore<User> _emailStore;
	private readonly ILogger _logger;
	private readonly IEmailSender _emailSender;

	public RegisterModel(
		[NotNull] UserManager userManager,
		[NotNull] IUserStore<User> userStore,
		[NotNull] SignInManager signInManager,
		[NotNull] ILogger<RegisterModel> logger,
		[NotNull] IEmailSender emailSender)
	{
		_userManager = userManager;
		_userStore = userStore;
		_emailStore = GetEmailStore();
		_signInManager = signInManager;
		_logger = logger;
		_emailSender = emailSender;
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
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
		[Display(Name = "البريد الالكتروني")]
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "كلمة المرور")]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[DataType(DataType.Password)]
		[Display(Name = "تأكبد كلمة المرور")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required]
		[StringLength(256)]
		[Display(Name = "الاسم الأول")]
		public string FirstName { get; set; }

		[StringLength(256)]
		[Display(Name = "الاسم الأخير")]
		public string LastName { get; set; }

		[StringLength(256)]
		[Display(Name = "الكنية")]
		public string NickName { get; set; }

		[StringLength(320)]
		[Display(Name = "الصورة")]
		public string ImageUrl { get; set; }
		[Display(Name = "النوع")]
		public Genders Gender { get; set; }
		[Display(Name = "تاريخ الميلاد")]
		public DateTime? DateOfBirth { get; set; }
	}


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
		if (ModelState.IsValid)
		{
			User user = CreateUser();

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
												$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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
		}

		// If we got this far, something failed, redisplay form
		return Page();
	}

	private User CreateUser()
	{
		try
		{
			return Activator.CreateInstance<User>();
		}
		catch
		{
			throw new InvalidOperationException($"Can't create an instance of '{nameof(Model.Entities.User)}'. " +
												$"Ensure that '{nameof(Model.Entities.User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
												$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
		}
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