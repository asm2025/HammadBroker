using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Repositories;

public interface IBuildingAdRepository : IRepository<DataContext, BuildingAd, int>
{
	IQueryable<BuildingAd> ListActive(IPagination settings = null);
	[NotNull]
	Task<IList<BuildingAd>> ListActiveAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));
}