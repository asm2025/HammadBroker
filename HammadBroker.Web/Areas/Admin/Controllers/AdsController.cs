using essentialMix.Core.Web.Controllers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route($"{nameof(Admin)}/[controller]")]
public class AdsController : MvcController
{
	/// <inheritdoc />
	public AdsController([NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<AdsController> logger)
		: base(configuration, environment, logger)
	{
	}

	[NotNull]
	public IActionResult Index()
	{
		return View();
	}
}