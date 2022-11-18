using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class AdRepository : Repository<DataContext, Ad, long>, IAdRepository
{
	/// <inheritdoc />
	public AdRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<AdRepository> logger)
		: base(context, configuration, logger)
	{
	}
}