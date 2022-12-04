using System.Threading;
using System.Threading.Tasks;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface IBuildingAdService : IService<DataContext, IBuildingAdRepository, BuildingAd, int>
{
	[NotNull]
	[ItemNotNull]
	Task<IPaginated<BuildingAdForList>> ListWithBuildingsAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken));
}