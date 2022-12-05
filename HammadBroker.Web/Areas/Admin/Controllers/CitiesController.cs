using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Services;
using HammadBroker.Model.DTO;
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

	/// <inheritdoc />
	public CitiesController([NotNull] ICityService cityService, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<CitiesController> logger)
		: base(configuration, environment, logger)
	{
		_cityService = cityService;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index(CitiesList pagination, CancellationToken token)
	{
		pagination ??= new CitiesList();
		IPaginated<CityForList> result = await _cityService.ListAsync<CityForList>(pagination, token);
		return View(result);
	}
}