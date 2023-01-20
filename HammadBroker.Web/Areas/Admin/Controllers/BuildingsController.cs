using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
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
using SysFile = System.IO.File;

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
	private readonly string _assetImagesBaseUrl;

	/// <inheritdoc />
	public BuildingsController([NotNull] IBuildingService buildingService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IMapper mapper, [NotNull] VirtualPathSettings virtualPathSettings, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<BuildingsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
		_mapper = mapper;
		PathContent assetsPath = virtualPathSettings.PathContents?.FirstOrDefault(e => e.Alias.IsSame("AssetImages")) ?? throw new ConfigurationErrorsException($"{nameof(VirtualPathSettings)} does not contain a definition for AssetImages.");
		_assetImagesPath = Path.Combine(environment.WebRootPath, assetsPath.PhysicalPath).Suffix(Path.DirectorySeparatorChar);
		_assetImagesBaseUrl = assetsPath.RequestPath.Suffix(Path.AltDirectorySeparatorChar);
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
		token.ThrowIfCancellationRequested();
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[Authorize]
	[HttpGet("[action]")]
	public async Task<IActionResult> List([FromQuery(Name = "")] BuildingList settings, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		IList<BuildingForList> result = await _buildingService.ListAsync(settings, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
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
		token.ThrowIfCancellationRequested();
		await _lookupService.FillCityNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		return View(building);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> GetById(int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingForDetails building = await _buildingService.GetAsync<BuildingForDetails>(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		await _lookupService.FillCountryNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		await _lookupService.FillCityNameAsync(building, token);
		token.ThrowIfCancellationRequested();
		return Ok(building);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(string countryCode, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(countryCode)) countryCode = _companyInfo.CountryCode;
		BuildingToUpdate buildingToUpdate = new BuildingToUpdate
		{
			CountryCode = countryCode
		};
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([NotNull, FromForm(Name = "")] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingToUpdate);

		Building building = await _buildingService.AddAsync(_mapper.Map<Building>(buildingToUpdate), token);
		token.ThrowIfCancellationRequested();
		if (building == null) return BadRequest();

		bool update = false;
		IFormFile formFile = buildingToUpdate.Image;

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
	[HttpGet("[action]")]
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
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit([Required] int id, [NotNull, FromForm(Name = "")] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingToUpdate);

		Building building = await _buildingService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return NotFound();
		_mapper.Map(buildingToUpdate, building);
		building = await _buildingService.UpdateAsync(building, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return BadRequest();

		bool update = false;
		IFormFile formFile = buildingToUpdate.Image;

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

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> ListImages(int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		IList<string> images = await _buildingService.ListImagesAsync(id, token);

		for (int i = 0; i < images.Count; i++)
		{
			images[i] = _assetImagesBaseUrl + images[i];
		}

		return Ok(images);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> AddImage([Required] int id, BuildingImageToAdd imageToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (imageToAdd?.Image == null) ModelState.AddModelError(string.Empty, "لم يتم اختيار صورة للتحميل");
		else if (imageToAdd.Image.Length == 0) ModelState.AddModelError(string.Empty, $"الصورة '{imageToAdd.Image.FileName}' غير صالحة للتحميل.");

		if (imageToAdd?.Image == null || !ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			Building building = await _buildingService.GetAsync(id, token);
			token.ThrowIfCancellationRequested();
			if (building == null) return NotFound();

			string fileName = UploadBuildingImage(_assetImagesPath, building.Id, imageToAdd.Image);
			if (string.IsNullOrEmpty(fileName)) return Problem($"حدث خطأ أثناء تحميل الصورة '{imageToAdd.Image.FileName}'.");
			await _buildingService.AddImageAsync(building, Path.GetFileName(fileName), token);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return Problem(ex.Unwrap());
		}

		return Ok();
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteImage([Required] int id, [Required] int imageId, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		try
		{
			Building building = await _buildingService.GetAsync(id, token);
			token.ThrowIfCancellationRequested();
			if (building == null) return NotFound();

			BuildingImage buildingImage = await _buildingService.DeleteBuildingImageAsync(imageId, token);
			if (buildingImage == null) return Problem($"Building image with id {imageId} is not found.");

			string fileName = Path.Combine(_assetImagesPath, buildingImage.ImageUrl);
			if (SysFile.Exists(fileName)) FileHelper.Delete(fileName);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return Problem(ex.Unwrap());
		}

		return Ok();
	}

	private static string UploadBuildingImage(string path, int id, [NotNull] IFormFile formFile)
	{
		Stream stream = null;
		Image image = null;
		Image thumb = null;
		string fileName = Path.Combine(path, $"{id}.{Guid.NewGuid():N}{Path.GetExtension(formFile.FileName).Prefix('.')}");

		try
		{
			stream = formFile.OpenReadStream();
			image = Image.FromStream(stream);
			thumb = image.Width > Constants.Images.DimensionMax || image.Height > Constants.Images.DimensionMax
						? ImageHelper.Resize(image, Constants.Images.DimensionMax, image.Width >= image.Height)
						: image;
			if (SysFile.Exists(fileName)) FileHelper.Delete(fileName);
			fileName = ImageHelper.Save(thumb, fileName);
			return fileName;
		}
		finally
		{
			ObjectHelper.Dispose(ref thumb);
			ObjectHelper.Dispose(ref image);
			ObjectHelper.Dispose(ref stream);
		}
	}
}