// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using AutoMapper;
using HammadBroker.Data.Identity;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HammadBroker.Web.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;
	private readonly IMapper _mapper;

	public IndexModel([NotNull] UserManager userManager, [NotNull] SignInManager signInManager, [NotNull] IMapper mapper)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_mapper = mapper;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string Username { get; set; }
	public string Email { get; set; }

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
	public UserToUpdate Input { get; set; }

	[ItemNotNull]
	public async Task<IActionResult> OnGetAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null) return NotFound();
		Username = user.UserName;
		Email = user.Email;
		Input = _mapper.Map<UserToUpdate>(user);
		return Page();
	}

	[ItemNotNull]
	public async Task<IActionResult> OnPostAsync()
	{
		User user = await _userManager.GetUserAsync(User);
		if (user == null) return NotFound();
		Username = user.UserName;
		Email = user.Email;

		if (!ModelState.IsValid)
		{
			Input = _mapper.Map<UserToUpdate>(user);
			return Page();
		}

		string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
		_mapper.Map(Input, user);
		if (phoneNumber != Input.PhoneNumber) Input.PhoneNumberConfirmed = false;
		await _userManager.UpdateAsync(user);
		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Your profile has been updated";
		return RedirectToPage();
	}
}