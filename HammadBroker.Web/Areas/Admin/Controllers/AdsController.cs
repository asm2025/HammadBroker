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
[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
public class AdsController : MvcController
{
	private readonly IBuildingAdService _buildingAdService;
	private readonly ILookupService _lookupService;
	private readonly CompanyInfo _companyInfo;
	private readonly IMapper _mapper;

	/// <inheritdoc />
	public AdsController([NotNull] IBuildingAdService buildingAdService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<AdsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingAdService = buildingAdService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
		_mapper = mapper;
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

		IPaginated<BuildingAdForList> paginated = await _buildingAdService.ListAsync<BuildingAdForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		BuildingAdsPaginated result = new BuildingAdsPaginated(paginated.Result, (BuildingAdList)paginated.Pagination);
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("{id:int}")]
	public async Task<IActionResult> Get(int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingAdForDetails building = await _buildingAdService.GetAsync<BuildingAdForDetails>(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		await _lookupService.FillCountryNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		await _lookupService.FillCityNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		building.Images = await _lookupService.ListBuildingImagesAsync(id, 0, token);
		token.ThrowIfCancellationRequested();
		return View(building);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(int buildingId, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		DateTime today = DateTime.Now;
		BuildingAdToUpdate buildingToUpdate = new BuildingAdToUpdate
		{
			BuildingId = buildingId,
			Date = today,
			Expires = today.AddMonths(1).AddDays(-1),
			Phone = _companyInfo.Phone,
			Mobile = _companyInfo.Mobile,
		};
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([NotNull] BuildingAdToUpdate buildingAdToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingAdToUpdate);

		BuildingAd buildingAd = await _buildingAdService.AddAsync(_mapper.Map<BuildingAd>(buildingAdToUpdate), token);
		token.ThrowIfCancellationRequested();
		if (buildingAd == null) return BadRequest();
		return RedirectToAction(nameof(Get), new
		{
			id = buildingAd.Id,
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("{id:int}/[action]")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingAdToUpdate buildingAdToUpdate = await _buildingAdService.GetAsync<BuildingAdToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (buildingAdToUpdate == null) return NotFound();
		return View(buildingAdToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("{id:int}/[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull] BuildingAdToUpdate buildingAdToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingAdToUpdate);

		BuildingAd buildingAd = await _buildingAdService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (buildingAd == null) return NotFound();
		_mapper.Map(buildingAdToUpdate, buildingAd);
		buildingAd = await _buildingAdService.UpdateAsync(buildingAd, token);
		token.ThrowIfCancellationRequested();
		if (buildingAd == null) return BadRequest();
		return RedirectToAction(nameof(Get), new
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
		BuildingAd buildingAd = await _buildingAdService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (buildingAd == null) return NotFound();

		if (!string.IsNullOrEmpty(returnUrl))
		{
			returnUrl = WebUtility.UrlDecode(returnUrl);
			if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
		}

		return RedirectToAction(nameof(Index));
	}
}