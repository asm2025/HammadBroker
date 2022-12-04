using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Services;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Parameters;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
public class AdsController : MvcController
{
	private readonly IBuildingAdService _buildingAdService;
	private readonly string _buildingImagesPath;

	/// <inheritdoc />
	public AdsController([NotNull] IBuildingAdService buildingAdService, [NotNull] IEnumerable<IFileProvider> fileProviders, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<AdsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingAdService = buildingAdService;
		_buildingImagesPath = fileProviders.First(e => e.Alias == "AssetImages").Root;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index(BuildingList pagination, CancellationToken token)
	{
		pagination ??= new BuildingList();
		IPaginated<BuildingAdForList> result = await _buildingAdService.ListAsync<BuildingAdForList>(pagination, token);
		return View(result);
	}
}