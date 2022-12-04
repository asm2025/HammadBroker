using essentialMix.Core.Web.Controllers;
using HammadBroker.Data.Services;
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
	public IActionResult Index()
	{
		return View();
	}
}