using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
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
			pagination.OrderBy.Add(new SortField(nameof(BuildingAd.AdType)));
		}

		BuildingAdsPaginated result = await _buildingAdService.ListWithBuildingsAsync(pagination, token);
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
	public async Task<IActionResult> Add([NotNull, FromForm(Name = nameof(BuildingAdModel.Ad))] BuildingAdToUpdate buildingAdToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingAdToUpdate);

		BuildingAd buildingAd = await _buildingAdService.AddAsync(_mapper.Map<BuildingAd>(buildingAdToUpdate), token);
		token.ThrowIfCancellationRequested();
		if (buildingAd == null) return BadRequest();
		return RedirectToAction(nameof(Index));
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
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
	[HttpPost("[action]")]
	public async Task<IActionResult> Edit([Required, FromQuery] int id, [NotNull, FromForm(Name = nameof(BuildingAdModel.Ad))] BuildingAdToUpdate buildingAdToUpdate, CancellationToken token)
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
	[HttpPost("[action]")]
	public async Task<IActionResult> Delete([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		BuildingAd buildingAd = await _buildingAdService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		return buildingAd == null
					? NotFound()
					: Ok();
	}
}