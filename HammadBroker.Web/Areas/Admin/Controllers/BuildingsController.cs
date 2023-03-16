﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
using essentialMix.Drawing.Helpers;
using essentialMix.Extensions;
using essentialMix.Helpers;
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
using NToastNotify;
using SysFile = System.IO.File;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
public class BuildingsController : MvcController
{
	private readonly IBuildingService _buildingService;
	private readonly ILookupService _lookupService;
	private readonly CompanyInfo _companyInfo;
	private readonly IMapper _mapper;
	private readonly IToastNotification _toastNotification;
	private readonly string _assetImagesPath;
	private readonly string _assetImagesBaseUrl;

	/// <inheritdoc />
	public BuildingsController([NotNull] IBuildingService buildingService, [NotNull] ILookupService lookupService, [NotNull] CompanyInfo companyInfo, [NotNull] IMapper mapper, [NotNull] VirtualPathSettings virtualPathSettings, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] IToastNotification toastNotification, [NotNull] ILogger<BuildingsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_lookupService = lookupService;
		_companyInfo = companyInfo;
		_mapper = mapper;
		_toastNotification = toastNotification;
		PathContent assetsPath = virtualPathSettings.PathContents?.FirstOrDefault(e => e.Alias.IsSame("AssetImages")) ?? throw new ConfigurationErrorsException($"{nameof(VirtualPathSettings)} does not contain a definition for AssetImages.");
		_assetImagesPath = Path.Combine(environment.WebRootPath, assetsPath.PhysicalPath).Suffix(Path.DirectorySeparatorChar);
		_assetImagesBaseUrl = assetsPath.RequestPath.Suffix(Path.AltDirectorySeparatorChar);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>();
			if (pagination.CityId < 1) pagination.OrderBy.Add(new SortField(nameof(Building.CityId)));
			if (!pagination.Date.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.Date), SortType.Descending));
			if (!pagination.AdType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.AdType)));
			if (!pagination.BuildingType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.BuildingType)));
			if (!pagination.FinishingType.HasValue) pagination.OrderBy.Add(new SortField(nameof(Building.FinishingType)));
		}

		BuildingsPaginated<BuildingForList> result = await _buildingService.ListAsync<BuildingForList>(pagination, token);
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
		IList<BuildingForList> result = await _buildingService.LookupAsync<BuildingForList>(settings, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Get([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		BuildingForDetails building = await _buildingService.GetAsync<BuildingForDetails>(id, token);
		token.ThrowIfCancellationRequested();
		if (building == null) return Problem("الاعلان غير موجود.");
		await _lookupService.FillAddressLookupAsync(building, token);
		token.ThrowIfCancellationRequested();
		return View(building);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		DateTime today = DateTime.Now;
		return View(new BuildingToUpdate
		{
			Date = today,
			Expires = today.AddMonths(1).AddDays(-1),
			Phone = _companyInfo.Phone,
			Mobile = _companyInfo.Mobile,
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Add([NotNull, FromForm(Name = nameof(BuildingModel.Building))] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingToUpdate);

		Building building = await _buildingService.AddAsync(_mapper.Map<Building>(buildingToUpdate), token);
		token.ThrowIfCancellationRequested();

		if (building == null)
		{
			_toastNotification.AddErrorToastMessage("تعذر اضافة الاعلان. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم اضافة الاعلان '{building.Reference}' بنجاح.");
		return RedirectToAction(nameof(Get), new
		{
			building.Id
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		BuildingToUpdate buildingToUpdate = await _buildingService.GetAsync<BuildingToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (buildingToUpdate == null) return Problem("الاعلان غير موجود.");
		return View(buildingToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Edit([Required] int id, [NotNull, FromForm(Name = nameof(BuildingModel.Building))] BuildingToUpdate buildingToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(buildingToUpdate);

		Building building = await _buildingService.GetAsync(id, token);

		if (building == null)
		{
			_toastNotification.AddErrorToastMessage("الاعلان غير موجود.");
			return BadRequest();
		}

		DateTime createdOn = building.CreatedOn;
		_mapper.Map(buildingToUpdate, building);
		building.CreatedOn = createdOn;
		building = await _buildingService.UpdateAsync(building, token);
		token.ThrowIfCancellationRequested();

		if (building == null)
		{
			_toastNotification.AddErrorToastMessage("تعذر تعديل الاعلان. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم تعديل الاعلان '{building.Reference}' بنجاح.");
		return RedirectToAction(nameof(Get), new
		{
			building.Id
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Delete([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		Building building = await _buildingService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (building == null)
		{
			_toastNotification.AddErrorToastMessage("الاعلان غير موجود.");
			return Problem("الاعلان غير موجود.");
		}

		_toastNotification.AddSuccessToastMessage($"تم حذف الاعلان '{building.Reference}' بنجاح.");
		return Ok();
	}

	[NotNull]
	[ItemNotNull]
	[AllowAnonymous]
	[Authorize(Policy = Constants.Authorization.MemberPolicy)]
	[HttpGet("[action]")]
	public async Task<IActionResult> ListImages([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		IList<BuildingImage> images = await _buildingService.ListImagesAsync(id, token);

		foreach (BuildingImage img in images)
		{
			if (string.IsNullOrEmpty(img.ImageUrl)) continue;
			img.ImageUrl = _assetImagesBaseUrl + img.ImageUrl;
		}

		return Ok(images);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> AddImage([Required, FromQuery] int id, [NotNull] BuildingImageToAdd imageToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		if (imageToAdd.Image == null) ModelState.AddModelError(string.Empty, "لم يتم اختيار صورة للتحميل");
		else if (imageToAdd.Image.Length == 0) ModelState.AddModelError(string.Empty, $"الصورة '{imageToAdd.Image.FileName}' غير صالحة للتحميل.");

		if (imageToAdd.Image == null || !ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			Building building = await _buildingService.GetAsync(id, token);
			token.ThrowIfCancellationRequested();

			if (building == null)
			{
				_toastNotification.AddErrorToastMessage("الاعلان غير موجود.");
				return Problem("الاعلان غير موجود.");
			}

			string fileName = UploadBuildingImage(_assetImagesPath, building.Id, imageToAdd.Image);

			if (string.IsNullOrEmpty(fileName))
			{
				string msg = $"حدث خطأ أثناء تحميل الصورة '{imageToAdd.Image.FileName}'.";
				_toastNotification.AddErrorToastMessage(msg);
				return Problem(msg);
			}

			await _buildingService.AddImageAsync(new BuildingImage
			{
				BuildingId = id,
				ImageUrl = Path.GetFileName(fileName),
				Priority = imageToAdd.Priority,
			}, token);
			_toastNotification.AddSuccessToastMessage($"تم اضافة الصورة '{imageToAdd.Image.FileName}' الى الملف '{fileName}' بنجاح.");
		}
		catch (Exception ex)
		{
			string msg = ex.Unwrap();
			Logger.LogError(ex.CollectMessages());
			_toastNotification.AddErrorToastMessage(msg);
			return Problem(msg);
		}

		return Ok();
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> DeleteImage([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			BuildingImage buildingImage = await _buildingService.DeleteImageAsync(id, token);

			if (buildingImage == null)
			{
				string msg = $"Building image with id {id} is not found.";
				_toastNotification.AddErrorToastMessage(msg);
				return Problem(msg);
			}

			_toastNotification.AddSuccessToastMessage($"تم حذف الصورة '{buildingImage.ImageUrl}' بنجاح.");
		}
		catch (Exception ex)
		{
			string msg = ex.Unwrap();
			Logger.LogError(ex.CollectMessages());
			_toastNotification.AddErrorToastMessage(msg);
			return Problem(msg);
		}

		return Ok();
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> DeleteImages([Required] int[] id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			IList<BuildingImage> images = await _buildingService.DeleteImagesAsync(id, token);
			if (images.Count > 0) _toastNotification.AddSuccessToastMessage($"تم حذف الصور {string.Join(", ", images.Select(e => e.ImageUrl.SingleQuote()))} بنجاح.");
			return Ok();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return Problem(ex.Unwrap());
		}
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> DeleteAllImages([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			IList<string> images = await _buildingService.DeleteImagesAsync(id, token);
			if (images.Count > 0) _toastNotification.AddSuccessToastMessage($"تم حذف الصور {string.Join(", ", images.Select(e => e.SingleQuote()))} بنجاح.");
			return Ok();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return Problem(ex.Unwrap());
		}
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> SetImagePriority([Required] int id, byte? priority, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);

		try
		{
			BuildingImage image = await _buildingService.GetImageAsync(id, token);
			if (image == null) return BadRequest();
			image.Priority = priority;
			if (await _buildingService.UpdateImageAsync(image, token) == null) return BadRequest();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return Problem(ex.Unwrap());
		}

		return Ok();
	}

	private static string UploadBuildingImage([NotNull] string path, int id, [NotNull] IFormFile formFile)
	{
		Stream stream = null;
		Image image = null;
		Image thumb = null;
		string fileName = Path.Combine(path, $"{id}.{Guid.NewGuid():N}{Path.GetExtension(formFile.FileName).Prefix('.')}");

		try
		{
			stream = formFile.OpenReadStream();
			image = Image.FromStream(stream);
			thumb = image.Width > Constants.Images.DimensionXMax || image.Height > Constants.Images.DimensionYMax
						? ImageHelper.Resize(image, Constants.Images.DimensionXMax, Constants.Images.DimensionYMax)
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