using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Web.Controllers;
using essentialMix.Drawing.Helpers;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Repositories;
using HammadBroker.Data.Services;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
public class BuildingsController : MvcController
{
	private readonly IBuildingService _buildingService;
	private readonly ILookupService _lookupService;
	private readonly ICityRepository _cityRepository;
	private readonly IMapper _mapper;
	private readonly string _assetImagesPath;

	/// <inheritdoc />
	public BuildingsController([NotNull] IBuildingService buildingService, [NotNull] ILookupService lookupService, [NotNull] ICityRepository cityRepository, [NotNull] IMapper mapper, [NotNull] IEnumerable<IFileProvider> fileProviders, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<BuildingsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_lookupService = lookupService;
		_cityRepository = cityRepository;
		_mapper = mapper;
		_assetImagesPath = fileProviders.First(e => e.Alias == "AssetImages").Root;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingList();

		// There is a fucking bug in order by. it produces "ORDER BY (SELECT 1)"
		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(3);
			pagination.OrderBy.Add(new SortField(nameof(Building.CityId)));
			pagination.OrderBy.Add(new SortField(nameof(Building.BuildingType)));
			pagination.OrderBy.Add(new SortField(nameof(Building.FinishingType)));
		}

		IPaginated<BuildingForList> paginated = await _buildingService.ListAsync<BuildingForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		BuildingsPaginated result = new BuildingsPaginated(paginated.Result, paginated.Pagination);
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		return View(new BuildingToUpdate
		{
			CountryCode = countryCode,
			Countries = await _lookupService.ListCountriesAsync(token),
			Cities = string.IsNullOrEmpty(countryCode)
						? Array.Empty<CityForList>()
						: await _cityRepository.List()
												.Where(e => e.CountryCode == countryCode)
												.ProjectTo<CityForList>(_mapper.ConfigurationProvider)
												.ToListAsync(token)
		});
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
			buildingToUpdate.Countries = await _lookupService.ListCountriesAsync(token);
			token.ThrowIfCancellationRequested();

			if (!string.IsNullOrEmpty(buildingToUpdate.CountryCode))
			{
				buildingToUpdate.Cities = await _cityRepository.List()
																.Where(e => e.CountryCode == buildingToUpdate.CountryCode)
																.ProjectTo<CityForList>(_mapper.ConfigurationProvider)
																.ToListAsync(token);
			}
			else
			{
				buildingToUpdate.Cities = Array.Empty<CityForList>();
			}

			return View(buildingToUpdate);
		}

		Building building = _mapper.Map<Building>(buildingToUpdate);
		BuildingForList buildingForList = await _buildingService.AddAsync<BuildingForList>(building, token);
		token.ThrowIfCancellationRequested();
		if (buildingForList == null) return BadRequest();
		building.Id = buildingForList.Id;

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

		if (update) await _buildingService.UpdateAsync<BuildingForList>(building, token);
		return RedirectToAction(nameof(Edit), new
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
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingToUpdate);
		Building building = await _buildingService.Repository.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		_mapper.Map(buildingToUpdate, building);
		BuildingForList buildingForList = await _buildingService.UpdateAsync<BuildingForList>(building, token);
		token.ThrowIfCancellationRequested();
		if (buildingForList == null) return BadRequest();

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

		if (update) await _buildingService.UpdateAsync<BuildingForList>(building, token);
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
		Building building = await _buildingService.Repository.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		await _buildingService.Context.SaveChangesAsync(token);
		if (!string.IsNullOrEmpty(building.ImageUrl)) FileHelper.Delete(Path.Combine(_assetImagesPath, building.ImageUrl));
		return Ok();
	}
}