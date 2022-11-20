// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly ILogger _logger;

	public PersonalDataModel(
		[NotNull] UserManager userManager,
		[NotNull] ILogger<PersonalDataModel> logger)
	{
		_userManager = userManager;
		_logger = logger;
	}

	[ItemNotNull]
	public async Task<IActionResult> OnGet()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

		return Page();
	}
}