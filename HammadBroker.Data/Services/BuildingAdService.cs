using AutoMapper;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class BuildingAdService : Service<DataContext, IBuildingAdRepository, BuildingAd, int>, IBuildingAdService
{
	public BuildingAdService([NotNull] IBuildingAdRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<BuildingAdService> logger)
		: base(repository, mapper, logger)
	{
	}
}