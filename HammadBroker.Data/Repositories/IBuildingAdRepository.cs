using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Repositories;

public interface IBuildingAdRepository : IRepository<DataContext, BuildingAd, int>
{
	[NotNull]
	[ItemNotNull]
	IQueryable<BuildingAdForList> ListWithBuildings(IPagination settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken));
}