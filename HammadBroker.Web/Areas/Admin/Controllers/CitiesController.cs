using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
public class CitiesController : MvcController
{
	private readonly ICityService _cityService;
	private readonly ILookupService _lookupService;
	private readonly IMapper _mapper;
	private readonly IToastifyService _toastifyService;

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] ILookupService lookupService, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] IToastifyService toastifyService, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
		_lookupService = lookupService;
		_mapper = mapper;
		_toastifyService = toastifyService;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] CitiesList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new CitiesList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(1);
			pagination.OrderBy.Add(new SortField(nameof(City.Name)));
		}

		IPaginated<CityForList> paginated = await _cityService.ListAsync<CityForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		CitiesPaginated result = new CitiesPaginated(paginated.Result, (CitiesList)paginated.Pagination);
		token.ThrowIfCancellationRequested();
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[AllowAnonymous]
	[Authorize(Policy = Constants.Authorization.MemberPolicy)]
	[HttpGet("[action]")]
	public async Task<IActionResult> List([FromQuery(Name = "")] CitiesList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!string.IsNullOrEmpty(pagination.Search)) pagination.Search = WebUtility.UrlDecode(pagination.Search);

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(1);
			pagination.OrderBy.Add(new SortField(nameof(City.Name)));
		}

		pagination.PageSize = 0;
		IList<CityForList> result = await _lookupService.ListCitiesAsync(pagination, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		return View(new CityToUpdate());
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Add([NotNull] CityToUpdate cityToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToAdd);

		City city = await _cityService.AddAsync(_mapper.Map<City>(cityToAdd), token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastifyService.Error("تعذر اضافة المدينة. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastifyService.Success($"تم اضافة المدينة '{city.Name}' بنجاح.");
		return RedirectToAction(nameof(Index));
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		CityToUpdate cityToUpdate = await _cityService.GetAsync<CityToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (cityToUpdate == null) return Problem("المدينة غير موجودة.");
		return View(cityToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Edit([Required, FromQuery] int id, [NotNull] CityToUpdate cityToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToUpdate);
		City city = await _cityService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastifyService.Error("المدينة غير موجودة.");
			return Problem("المدينة غير موجودة.");
		}

		_mapper.Map(cityToUpdate, city);
		city = await _cityService.UpdateAsync(city, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastifyService.Error("تعذر تحديث المدينة. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastifyService.Success($"تم تحديث المدينة '{city.Name}' بنجاح.");
		return RedirectToAction(nameof(Index));
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Delete([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		City city = await _cityService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastifyService.Error("المدينة غير موجودة او تعذر حذفها.");
			return Problem("المدينة غير موجودة او تعذر حذفها.");
		}

		_toastifyService.Success($"تم حذف المدينة '{city.Name}' بنجاح.");
		return Ok();
	}
}