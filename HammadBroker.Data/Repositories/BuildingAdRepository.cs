using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class BuildingAdRepository : Repository<DataContext, BuildingAd, int>, IBuildingAdRepository
{
	/// <inheritdoc />
	public BuildingAdRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingAdRepository> logger)
		: base(context, configuration, logger)
	{
	}
}