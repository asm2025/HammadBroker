using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
using essentialMix.Extensions;
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
[Authorize(Policy = Constants.Authorization.SystemPolicy)]
public class CitiesController : MvcController
{
	private readonly ICityService _cityService;
	private readonly ILookupService _lookupService;
	private readonly IMapper _mapper;

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] ILookupService lookupService, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
		_lookupService = lookupService;
		_mapper = mapper;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	public async Task<IActionResult> Index([FromQuery(Name = "")] CitiesList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new CitiesList();

		// There is a fucking bug in order by. it produces "ORDER BY (SELECT 1)"
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
			Countries = await _lookupService.ListCountriesAsync(token)
		};
		token.ThrowIfCancellationRequested();
		return View(result);
	}

	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> List(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) return Ok(Array.Empty<CityForList>());
		IPaginated<CityForList> result = await _cityService.ListAsync<CityForList>(_cityService.Context.Cities.Where(e => e.CountryCode == countryCode), null, token);
		token.ThrowIfCancellationRequested();
		return Ok(result.Result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) countryCode = UICulture.Region()?.ThreeLetterISORegionName;
		return View(new CityToUpdate
		{
			CountryCode = countryCode,
			Countries = await _lookupService.ListCountriesAsync(token)
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([NotNull] CityToUpdate cityToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		if (!ModelState.IsValid)
		{
			cityToAdd.Countries = await _lookupService.ListCountriesAsync(token);
			token.ThrowIfCancellationRequested();
			return View(cityToAdd);
		}

		CityForList cityForList = await _cityService.AddAsync<CityForList>(_mapper.Map<City>(cityToAdd), token);
		token.ThrowIfCancellationRequested();
		if (cityForList == null) return BadRequest();
		return RedirectToAction(nameof(Edit), new
		{
			id = cityForList.Id,
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]/{id:int}")]
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
	[HttpPost("[action]/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull] CityToUpdate cityToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToUpdate);
		City city = await _cityService.Repository.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return NotFound();
		_mapper.Map(cityToUpdate, city);
		CityForList cityForList = await _cityService.UpdateAsync<CityForList>(city, token);
		token.ThrowIfCancellationRequested();
		if (cityForList == null) return BadRequest();
		return RedirectToAction(nameof(Edit), new
		{
			id
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpDelete("[action]")]
	public async Task<IActionResult> Delete([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		City city = await _cityService.Repository.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return NotFound();
		await _cityService.Context.SaveChangesAsync(token);
		return Ok();
	}
}