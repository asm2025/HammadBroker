using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Web.Controllers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Services;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Web.Areas.Admin.Controllers;

[Area(nameof(Admin))]
[Route("[area]/[controller]")]
[Authorize(Policy = Constants.Authorization.AdministrationPolicy)]
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
	public async Task<IActionResult> Index([FromQuery(Name = "")] BuildingAdList pagination, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		pagination ??= new BuildingAdList();

		if (pagination.OrderBy == null || pagination.OrderBy.Count == 0)
		{
			pagination.OrderBy ??= new List<SortField>(3);
			pagination.OrderBy.Add(new SortField(nameof(BuildingAd.Date), SortType.Descending));
			pagination.OrderBy.Add(new SortField(nameof(BuildingAd.Type)));
		}

		IPaginated<BuildingAdForList> paginated = await _buildingAdService.ListAsync<BuildingAdForList>(pagination, token);
		token.ThrowIfCancellationRequested();
		BuildingAdsPaginated result = new BuildingAdsPaginated(paginated.Result, (BuildingAdList)paginated.Pagination);
		return View(result);
	}
}