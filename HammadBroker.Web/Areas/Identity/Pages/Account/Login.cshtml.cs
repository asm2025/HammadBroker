// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly ILogger _logger;

	public LoginModel([NotNull] UserManager userManager, [NotNull] SignInManager signInManager, [NotNull] ILogger<LoginModel> logger)
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
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string ErrorMessage { get; set; }

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
		[Display(Name = "البريد الالكتروني / اسم المستخدم")]
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "كلمة المرور")]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Display(Name = "تذكرني؟")]
		public bool RememberMe { get; set; }
	}

	public async Task OnGetAsync(string returnUrl = null)
	{
		if (!string.IsNullOrEmpty(ErrorMessage))
		{
			ModelState.AddModelError(string.Empty, ErrorMessage);
		}

		returnUrl ??= Url.Content("~/");

		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		ReturnUrl = returnUrl;
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync(string returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");

		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		if (!ModelState.IsValid) return Page();

		ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email)
								?? await _userManager.FindByNameAsync(Input.Email);
		if (user == null)
		{
			ModelState.AddModelError(string.Empty, "محاولة تسجيل الدخول غير صحيحة.");
			return Page();
		}

		SignInResult result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, true);

		if (result.Succeeded)
		{
			_logger.LogInformation("User logged in.");
			return LocalRedirect(returnUrl);
		}
		if (result.RequiresTwoFactor)
		{
			return RedirectToPage("./LoginWith2fa", new
			{
				ReturnUrl = returnUrl,
				Input.RememberMe
			});
		}
		if (result.IsLockedOut)
		{
			_logger.LogWarning("حساب المستخدم محظور.");
			return RedirectToPage("./Lockout");
		}

		ModelState.AddModelError(string.Empty, "محاولة تسجيل الدخول غير صحيحة.");
		return Page();

		// If we got this far, something failed, redisplay form
	}
}