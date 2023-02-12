using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model.Configuration;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Controllers;

[Route("")]
public class HomeController : MvcController
{
	private readonly IBuildingAdService _buildingAdService;
	private readonly ILookupService _lookupService;
	private readonly CompanyInfo _companyInfo;

	/// <inheritdoc />
	public HomeController([NotNull] IBuildingAdService buildingAdService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<HomeController> logger)
		: base(configuration, environment, logger)
	{
		_buildingAdService = buildingAdService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingAdList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingAdList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(3);
			pagination.OrderBy.Add(new SortField(nameof(BuildingAd.Date), SortType.Descending));
			pagination.OrderBy.Add(new SortField(nameof(BuildingAd.Type)));
		}

		BuildingAdsForDisplayPaginated result = await _buildingAdService.ListActiveWithBuildingsAsync(pagination, token);
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Get(int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingAdForDetails building = await _buildingAdService.GetBuildingAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		await _lookupService.FillCountryNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		await _lookupService.FillCityNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		return View(building);
	}

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

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Cities([FromQuery(Name = "")] CitiesList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(pagination.CountryCode)) pagination.CountryCode = _companyInfo.CountryCode;
		if (string.IsNullOrEmpty(pagination.CountryCode)) return Ok(Array.Empty<CityForList>());
		if (!string.IsNullOrEmpty(pagination.Search)) pagination.Search = WebUtility.UrlDecode(pagination.Search);

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(2);
			if (string.IsNullOrEmpty(pagination.CountryCode)) pagination.OrderBy.Add(new SortField(nameof(City.CountryCode)));
			pagination.OrderBy.Add(new SortField(nameof(City.Name)));
		}

		pagination.PageSize = 0;
		IList<CityForList> result = await _lookupService.ListCitiesAsync(pagination, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
	}
}