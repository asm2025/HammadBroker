using essentialMix.Core.Web.Controllers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Controllers;

[Route("")]
public class HomeController : MvcController
{
	/// <inheritdoc />
	public HomeController([NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<HomeController> logger)
		: base(configuration, environment, logger)
	{
	}

	[HttpGet]
	[NotNull]
	public IActionResult Index()
	{
		return View();
	}
}