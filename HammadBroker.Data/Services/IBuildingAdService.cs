using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface IBuildingAdService : IService<DataContext, IBuildingAdRepository, BuildingAd, int>
{
	[NotNull]
	[ItemNotNull]
	Task<BuildingAdsPaginated> ListWithBuildingsAsync([NotNull] BuildingAdList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	[ItemNotNull]
	Task<BuildingAdsForDisplayPaginated> ListActiveWithBuildingsAsync([NotNull] BuildingAdList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken));
}