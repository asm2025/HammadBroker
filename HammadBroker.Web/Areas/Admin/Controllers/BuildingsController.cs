using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
using essentialMix.Drawing.Helpers;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model;
using HammadBroker.Model.Configuration;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
[Authorize(Policy = Constants.Authorization.SystemPolicy)]
public class BuildingsController : MvcController
{
	private readonly IBuildingService _buildingService;
	private readonly ILookupService _lookupService;
	private readonly CompanyInfo _companyInfo;
	private readonly IMapper _mapper;
	private readonly string _assetImagesPath;

	/// <inheritdoc />
	public BuildingsController([NotNull] IBuildingService buildingService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IMapper mapper, [NotNull] IEnumerable<IFileProvider> fileProviders, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<BuildingsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
		_mapper = mapper;
		_assetImagesPath = fileProviders.First(e => e.Alias == "AssetImages").Root;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(3);
			pagination.OrderBy.Add(new SortField(nameof(Building.CityId)));
			pagination.OrderBy.Add(new SortField(nameof(Building.BuildingType)));
			pagination.OrderBy.Add(new SortField(nameof(Building.FinishingType)));
		}

		IPaginated<BuildingForList> paginated = await _buildingService.ListAsync<BuildingForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		BuildingsPaginated result = new BuildingsPaginated(paginated.Result, (BuildingList)paginated.Pagination);
		await _lookupService.FillCountriesAsync(result.Pagination, token);
		await _lookupService.FillCitiesAsync(result.Pagination, token);
		token.ThrowIfCancellationRequested();
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
		if (building == null) return NotFound();
		await _lookupService.FillCountryNameAsync(building, token);
		await _lookupService.FillCityNameAsync(building, token);
		await _lookupService.FillBuildingImagesAsync(id, building, 0, token);
		token.ThrowIfCancellationRequested();
		return View(building);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) countryCode = _companyInfo.CountryCode;
		BuildingToUpdate buildingToUpdate = new BuildingToUpdate
		{
			CountryCode = countryCode
		};
		await _lookupService.FillCountriesAsync(buildingToUpdate, token);
		await _lookupService.FillCitiesAsync(buildingToUpdate, token);
		token.ThrowIfCancellationRequested();
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([NotNull] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		if (!ModelState.IsValid)
		{
			await _lookupService.FillCountriesAsync(buildingToUpdate, token);
			await _lookupService.FillCitiesAsync(buildingToUpdate, token);
			token.ThrowIfCancellationRequested();
			return View(buildingToUpdate);
		}

		Building building = await _buildingService.AddAsync(_mapper.Map<Building>(buildingToUpdate), token);
		token.ThrowIfCancellationRequested();
		if (building == null) return BadRequest();

		bool update = false;
		IFormFile formFile = buildingToUpdate.ImageFile;

		if (formFile is { Length: > 0 })
		{
			Stream stream = null;
			Image image = null;
			Image thumb = null;
			string fileName = Path.Combine(_assetImagesPath, $"{building.Id}{Path.GetExtension(formFile.FileName).Prefix('.')}");

			try
			{
				stream = formFile.OpenReadStream();
				image = Image.FromStream(stream);
				thumb = image.Width > Constants.Images.DimensionMax || image.Height > Constants.Images.DimensionMax
							? ImageHelper.Resize(image, Constants.Images.DimensionMax, image.Width >= image.Height)
							: image;
				if (!string.IsNullOrEmpty(building.ImageUrl)) FileHelper.Delete(Path.Combine(_assetImagesPath, building.ImageUrl));
				fileName = ImageHelper.Save(thumb, fileName);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
				fileName = null;
			}
			finally
			{
				ObjectHelper.Dispose(ref thumb);
				ObjectHelper.Dispose(ref image);
				ObjectHelper.Dispose(ref stream);
			}

			update = !string.IsNullOrEmpty(fileName);
			if (!update) return BadRequest();
			building.ImageUrl = Path.GetFileName(fileName);
		}

		if (update && await _buildingService.UpdateAsync(building, token) == null) return Problem("تعذر حفظ الصورة بعد حفظ العقار");
		return RedirectToAction(nameof(Get), new
		{
			id = building.Id,
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]/{id:int}")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingToUpdate buildingToUpdate = await _buildingService.GetAsync<BuildingToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (buildingToUpdate == null) return NotFound();
		await _lookupService.FillCountriesAsync(buildingToUpdate, token);
		await _lookupService.FillCitiesAsync(buildingToUpdate, token);
		token.ThrowIfCancellationRequested();
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		if (!ModelState.IsValid)
		{
			await _lookupService.FillCountriesAsync(buildingToUpdate, token);
			await _lookupService.FillCitiesAsync(buildingToUpdate, token);
			token.ThrowIfCancellationRequested();
			return View(buildingToUpdate);
		}

		Building building = await _buildingService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		_mapper.Map(buildingToUpdate, building);
		building = await _buildingService.UpdateAsync(building, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return BadRequest();

		bool update = false;
		IFormFile formFile = buildingToUpdate.ImageFile;

		if (formFile is { Length: > 0 })
		{
			Stream stream = null;
			Image image = null;
			Image thumb = null;
			string fileName = Path.Combine(_assetImagesPath, $"{building.Id}{Path.GetExtension(formFile.FileName).Prefix('.')}");

			try
			{
				stream = formFile.OpenReadStream();
				image = Image.FromStream(stream);
				thumb = image.Width > Constants.Images.DimensionMax || image.Height > Constants.Images.DimensionMax
							? ImageHelper.Resize(image, Constants.Images.DimensionMax, image.Width >= image.Height)
							: image;
				if (!string.IsNullOrEmpty(building.ImageUrl)) FileHelper.Delete(Path.Combine(_assetImagesPath, building.ImageUrl));
				fileName = ImageHelper.Save(thumb, fileName);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
				fileName = null;
			}
			finally
			{
				ObjectHelper.Dispose(ref thumb);
				ObjectHelper.Dispose(ref image);
				ObjectHelper.Dispose(ref stream);
			}

			update = !string.IsNullOrEmpty(fileName);
			if (!update) return BadRequest();
			building.ImageUrl = Path.GetFileName(fileName);
		}

		if (update && await _buildingService.UpdateAsync(building, token) == null) return Problem("تعذر حفظ الصورة بعد حفظ العقار");
		return RedirectToAction(nameof(Get), new
		{
			id
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete([Required] int id, string returnUrl, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		Building building = await _buildingService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		if (!string.IsNullOrEmpty(building.ImageUrl)) FileHelper.Delete(Path.Combine(_assetImagesPath, building.ImageUrl));

		if (!string.IsNullOrEmpty(returnUrl))
		{
			returnUrl = WebUtility.UrlDecode(returnUrl);
			if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
		}

		return RedirectToAction(nameof(Index));
	}
}