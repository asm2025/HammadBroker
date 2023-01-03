using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model;
using HammadBroker.Model.Configuration;
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
[Authorize(Policy = Constants.Authorization.SystemPolicy)]
public class CitiesController : MvcController
{
	private readonly ICityService _cityService;
	private readonly ILookupService _lookupService;
	private readonly CompanyInfo _companyInfo;
	private readonly IMapper _mapper;

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
		_mapper = mapper;
	}

	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] CitiesList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new CitiesList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(2);
			if (string.IsNullOrEmpty(pagination.CountryCode)) pagination.OrderBy.Add(new SortField(nameof(City.CountryCode)));
			pagination.OrderBy.Add(new SortField(nameof(City.Name)));
		}

		IPaginated<CityForList> paginated = await _cityService.ListAsync<CityForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		CitiesPaginated result = new CitiesPaginated(paginated.Result, (CitiesList)paginated.Pagination)
		{
			Countries = await _lookupService.ListCountriesAsync(pagination.Search, token)
		};
		token.ThrowIfCancellationRequested();
		return View(result);
	}

	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Countries(string search, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!string.IsNullOrEmpty(search)) search = WebUtility.UrlDecode(search);
		IList<CountryForList> result = await _lookupService.ListCountriesAsync(search, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
	}

	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> List(string countryCode, string search, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) countryCode = _companyInfo.CountryCode;
		if (string.IsNullOrEmpty(countryCode)) return Ok(Array.Empty<CityForList>());
		if (!string.IsNullOrEmpty(search)) search = WebUtility.UrlDecode(search);
		IPaginated<CityForList> result = await _cityService.ListAsync<CityForList>(countryCode, search, null, token);
		token.ThrowIfCancellationRequested();
		return Ok(result.Result);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) countryCode = _companyInfo.CountryCode;
		CityToUpdate cityToUpdate = new CityToUpdate
		{
			CountryCode = countryCode
		};
		return View(cityToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([NotNull] CityToUpdate cityToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToAdd);

		City city = await _cityService.AddAsync(_mapper.Map<City>(cityToAdd), token);
		token.ThrowIfCancellationRequested();
		if (city == null) return BadRequest();
		return RedirectToAction(nameof(Edit), new
		{
			id = city.Id,
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("{id:int}/[action]")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		CityToUpdate cityToUpdate = await _cityService.GetAsync<CityToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (cityToUpdate == null) return NotFound();
		return View(cityToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("{id:int}/[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull] CityToUpdate cityToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToUpdate);
		City city = await _cityService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return NotFound();
		_mapper.Map(cityToUpdate, city);
		city = await _cityService.UpdateAsync(city, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return BadRequest();
		return RedirectToAction(nameof(Edit), new
		{
			id
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("{id:int}/[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([Required] int id, string returnUrl, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		City city = await _cityService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return NotFound();

		if (!string.IsNullOrEmpty(returnUrl))
		{
			returnUrl = WebUtility.UrlDecode(returnUrl);
			if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
		}

		return RedirectToAction(nameof(Index));
	}
}