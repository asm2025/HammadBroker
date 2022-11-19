﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
	private readonly SignInManager _signInManager;
	private readonly ILogger _logger;

	public LogoutModel([NotNull] SignInManager signInManager, [NotNull] ILogger<LogoutModel> logger)
	{
		_signInManager = signInManager;
		_logger = logger;
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPost(string returnUrl = null)
	{
		await _signInManager.SignOutAsync();
		_logger.LogInformation("User logged out.");
		if (returnUrl != null)
		{
			return LocalRedirect(returnUrl);
		}

		// This needs to be a redirect so that the browser performs a new
		// request and the identity for the user gets updated.
		return RedirectToPage();
	}
}