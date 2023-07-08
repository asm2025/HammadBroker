using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Data.Model;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Repositories;
using HammadBroker.Data.Services;
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
	private readonly IBuildingService _buildingService;
	private readonly ILookupService _lookupService;

	/// <inheritdoc />
	public HomeController([NotNull] IBuildingService buildingService, [NotNull] ILookupService lookupService, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<HomeController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_lookupService = lookupService;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingList();

		if (pagination.OrderBy is not { Count: > 0 })
		{
			pagination.OrderBy ??= new List<SortField>();
			pagination.OrderBy.Add(new SortField(nameof(Building.Priority), SortType.Descending));
			if (!pagination.AdType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.AdType)));
			if (!pagination.Date.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.Date), SortType.Descending));
			if (pagination.CityId < 1) pagination.OrderBy.Add(new SortField(nameof(Building.CityId)));
			if (pagination.DistrictId < 1) pagination.OrderBy.Add(new SortField(nameof(Building.DistrictId)));
			if (!pagination.BuildingType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.BuildingType)));
			if (!pagination.FinishingType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.FinishingType)));
		}

		BuildingsPaginated<BuildingForDisplay> result = await _buildingService.ListAsync<BuildingForDisplay>(pagination, token);
		token.ThrowIfCancellationRequested();
		await UpdateViews(result.Result);
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Get(int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingForDetails building = await _buildingService.GetAsync<BuildingForDetails>(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return Problem("الاعلان غير موجود.");
		await _lookupService.FillAddressLookupAsync(building, token);
		token.ThrowIfCancellationRequested();
		await UpdateView(building);
		return View(building);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Districts([FromQuery(Name = "")] DistrictList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new DistrictList();
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
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Cities([FromQuery(Name = "")] SearchList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new SearchList();
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

	private async Task UpdateViews<T>([NotNull] IEnumerable<T> result)
		where T : IEntity<int>, IBuildingLookup
	{
		IBuildingRepository repository = _buildingService.Repository;

		foreach (T buildingLookup in result)
		{
			Building building = await repository.GetAsync(buildingLookup.Id);
			if (building == null) continue;
			building.Views++;
			await repository.UpdateAsync(building);
		}

		await repository.Context.SaveChangesAsync();
	}

	private async Task UpdateView<T>([NotNull] T result)
		where T : IEntity<int>, IBuildingLookup
	{
		IBuildingRepository repository = _buildingService.Repository;
		Building building = await repository.GetAsync(result.Id);
		if (building == null) return;
		building.PageViews++;
		await repository.UpdateAsync(building);
		await repository.Context.SaveChangesAsync();
	}
}