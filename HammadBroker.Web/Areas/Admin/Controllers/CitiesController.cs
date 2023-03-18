using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Web.Controllers;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NToastNotify;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
public class CitiesController : MvcController
{
	private readonly ICityService _cityService;
	private readonly IDistrictService _districtService;
	private readonly ILookupService _lookupService;
	private readonly IMapper _mapper;
	private readonly IToastNotification _toastNotification;

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] IDistrictService districtService, [NotNull] ILookupService lookupService, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] IToastNotification toastNotification, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
		_districtService = districtService;
		_lookupService = lookupService;
		_mapper = mapper;
		_toastNotification = toastNotification;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] SearchList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new SearchList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(1);
			pagination.OrderBy.Add(new SortField(nameof(City.Name)));
		}

		IPaginated<CityForList> paginated = await _cityService.ListAsync<CityForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		CitiesPaginated result = new CitiesPaginated(paginated.Result, (SearchList)paginated.Pagination);
		token.ThrowIfCancellationRequested();
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[AllowAnonymous]
	[Authorize(Policy = Constants.Authorization.MemberPolicy)]
	[HttpGet("[action]")]
	public async Task<IActionResult> List([FromQuery(Name = "")] SearchList pagination, CancellationToken token)
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

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult Add(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		return View(new CityToUpdate());
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Add([NotNull] CityToUpdate cityToAdd, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToAdd);

		City city = await _cityService.AddAsync(_mapper.Map<City>(cityToAdd), token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastNotification.AddSuccessToastMessage("تعذر اضافة المدينة. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم اضافة المدينة '{city.Name}' بنجاح.");
		return RedirectToAction(nameof(Index));
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Edit([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		CityToUpdate cityToUpdate = await _cityService.GetAsync<CityToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (cityToUpdate == null) return Problem("المدينة غير موجودة.");
		return View(cityToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Edit([Required, FromQuery] int id, [NotNull] CityToUpdate cityToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(cityToUpdate);
		City city = await _cityService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastNotification.AddSuccessToastMessage("المدينة غير موجودة.");
			return Problem("المدينة غير موجودة.");
		}

		_mapper.Map(cityToUpdate, city);
		city = await _cityService.UpdateAsync(city, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastNotification.AddSuccessToastMessage("تعذر تحديث المدينة. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم تحديث المدينة '{city.Name}' بنجاح.");
		return RedirectToAction(nameof(Index));
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> Delete([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		City city = await _cityService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (city == null)
		{
			_toastNotification.AddSuccessToastMessage("المدينة غير موجودة او تعذر حذفها.");
			return Problem("المدينة غير موجودة او تعذر حذفها.");
		}

		_toastNotification.AddSuccessToastMessage($"تم حذف المدينة '{city.Name}' بنجاح.");
		return Ok();
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> Districts([FromQuery(Name = "")] DistrictList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new DistrictList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(2);
			if (pagination.CityId < 1) pagination.OrderBy.Add(new SortField(nameof(District.CityId)));
			pagination.OrderBy.Add(new SortField(nameof(District.Name)));
		}

		IPaginated<DistrictForList> paginated = await _districtService.ListAsync<DistrictForList>(pagination, token);
		token.ThrowIfCancellationRequested();

		if (pagination.CityId < 1)
		{
			IList<int> citiesIds = paginated.Result
											.Select(e => e.CityId)
											.Distinct()
											.ToList();

			if (citiesIds.Count > 0)
			{
				IDictionary<int, string> cities = await _cityService.Context.Cities
																	.Where(e => citiesIds.Contains(e.Id))
																	.ToDictionaryAsync(k => k.Id, v => v.Name, token);
				token.ThrowIfCancellationRequested();

				foreach (DistrictForList districtForList in paginated.Result)
				{
					if (!cities.TryGetValue(districtForList.CityId, out string cityName)) continue;
					districtForList.CityName = cityName;
				}
			}
		}

		DistrictsPaginated result = new DistrictsPaginated(paginated.Result, (DistrictList)paginated.Pagination);
		return View(result);
	}

	[NotNull]
	[ItemNotNull]
	[AllowAnonymous]
	[Authorize(Policy = Constants.Authorization.MemberPolicy)]
	[HttpGet("[action]")]
	public async Task<IActionResult> ListDistricts([FromQuery(Name = "")] DistrictList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new DistrictList();
		if (!string.IsNullOrEmpty(pagination.Search)) pagination.Search = WebUtility.UrlDecode(pagination.Search);

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(2);
			if (pagination.CityId < 1) pagination.OrderBy.Add(new SortField(nameof(District.CityId)));
			pagination.OrderBy.Add(new SortField(nameof(District.Name)));
		}

		pagination.PageSize = 0;
		IList<DistrictForList> result = await _lookupService.ListDistrictsAsync(pagination, token);
		token.ThrowIfCancellationRequested();
		return Ok(result);
	}

	[NotNull]
	[HttpGet("[action]")]
	public IActionResult AddDistrict(int cityId, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		return View(new DistrictToUpdate
		{
			CityId = cityId
		});
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> AddDistrict([NotNull] DistrictToUpdate districtToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(districtToUpdate);

		District district = await _districtService.AddAsync(_mapper.Map<District>(districtToUpdate), token);
		token.ThrowIfCancellationRequested();

		if (district == null)
		{
			_toastNotification.AddSuccessToastMessage("تعذر اضافة الحي. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم اضافة الحي '{district.Name}' بنجاح.");
		return RedirectToAction(nameof(Districts));
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet("[action]")]
	public async Task<IActionResult> EditDistrict([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		DistrictToUpdate districtToUpdate = await _districtService.GetAsync<DistrictToUpdate>(id, token);
		token.ThrowIfCancellationRequested();
		if (districtToUpdate == null) return Problem("الحي غير موجود.");
		return View(districtToUpdate);
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> EditDistrict([Required, FromQuery] int id, [NotNull] DistrictToUpdate districtToUpdate, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return View(districtToUpdate);
		District district = await _districtService.GetAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (district == null)
		{
			_toastNotification.AddSuccessToastMessage("الحي غير موجود.");
			return Problem("الحي غير موجود.");
		}

		_mapper.Map(districtToUpdate, district);
		district = await _districtService.UpdateAsync(district, token);
		token.ThrowIfCancellationRequested();

		if (district == null)
		{
			_toastNotification.AddSuccessToastMessage("تعذر تحديث الحي. برجاء المحاولة مرة اخرى بعد مراجعة الحقول المطلوبة");
			return BadRequest();
		}

		_toastNotification.AddSuccessToastMessage($"تم تحديث الحي '{district.Name}' بنجاح.");
		return RedirectToAction(nameof(Districts));
	}

	[NotNull]
	[ItemNotNull]
	[HttpPost("[action]")]
	public async Task<IActionResult> DeleteDistrict([Required] int id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!ModelState.IsValid) return BadRequest(ModelState);
		District district = await _districtService.DeleteAsync(id, token);
		token.ThrowIfCancellationRequested();

		if (district == null)
		{
			_toastNotification.AddSuccessToastMessage("الحي غير موجود او تعذر حذفه.");
			return Problem("الحي غير موجود او تعذر حذفه.");
		}

		_toastNotification.AddSuccessToastMessage($"تم حذف الحي '{district.Name}' بنجاح.");
		return Ok();
	}
}