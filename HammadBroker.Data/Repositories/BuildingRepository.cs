using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class BuildingRepository : Repository<DataContext, Building, int>, IBuildingRepository
{
	/// <inheritdoc />
	public BuildingRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingRepository> logger)
		: base(context, configuration, logger)
	{
	}
}