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
public class BuildingsController : MvcController
{
	private readonly IBuildingService _buildingService;
	private readonly string _buildingImagesPath;

	/// <inheritdoc />
	public BuildingsController([NotNull] IBuildingService buildingService, [NotNull] IEnumerable<IFileProvider> fileProviders, [NotNull] IConfiguration configuration, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<BuildingsController> logger)
		: base(configuration, environment, logger)
	{
		_buildingService = buildingService;
		_buildingImagesPath = fileProviders.First(e => e.Alias == "AssetImages").Root;
	}

	[NotNull]
	[ItemNotNull]
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingList pagination, CancellationToken token)
	{
		pagination ??= new BuildingList();
		IPaginated<BuildingForList> result = await _buildingService.ListAsync<BuildingForList>(pagination, token);
		return View(result);
	}
}