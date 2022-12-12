using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
public class CitiesController : MvcController
{
	private readonly ICityService _cityService;
	private readonly ILookupService _lookupService;

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] ILookupService lookupService, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
		_lookupService = lookupService;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
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

		IPaginated<CityForList> result = await _cityService.ListAsync<CityForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		CitiesPaginated paginated = new CitiesPaginated(result)
		{
			Countries = await _lookupService.ListCountriesAsync(token)
		};
		token.ThrowIfCancellationRequested();
		return View(paginated);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Add(string countryCode, CancellationToken token)
	{
		return View(new CityToUpdate
		{
			CountryCode = countryCode,
			Countries = await _lookupService.ListCountriesAsync(token)
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Add(CityToUpdate entity, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View();

		return View();
	}
}