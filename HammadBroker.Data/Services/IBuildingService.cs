using System.Collections.Generic;
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

public interface IBuildingService : IService<DataContext, IBuildingRepository, Building, string>
{
	[NotNull]
	BuildingsPaginated<T> List<T>(BuildingList settings = null)
		where T : class, IBuildingLookup;
	[NotNull]
	[ItemNotNull]
	Task<BuildingsPaginated<T>> ListAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken))
		where T : class, IBuildingLookup;
	[NotNull]
	IList<T> Lookup<T>(BuildingList settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IList<T>> LookupAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<BuildingImage> ListImages([NotNull] string buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImage>> ListImagesAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<T> ListImages<T>([NotNull] string buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<T>> ListImagesAsync<T>([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage([NotNull] string buildingId);
	[NotNull]
	Task<BuildingImage> GetImageAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage(int id);
	ValueTask<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage AddImage([NotNull] BuildingImage image);
	[ItemNotNull]
	ValueTask<BuildingImage> AddImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
	BuildingImage UpdateImage([NotNull] BuildingImage image);
	[NotNull]
	Task<BuildingImage> UpdateImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
	BuildingImage DeleteImage(int id);
	[NotNull]
	Task<BuildingImage> DeleteImageAsync(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage DeleteImage([NotNull] BuildingImage image);
	[NotNull]
	[ItemNotNull]
	Task<BuildingImage> DeleteImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
	void DeleteImages([NotNull] string buildingId);
	[NotNull]
	Task DeleteImagesAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<BuildingImage> DeleteImages([NotNull] int[] id);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImage>> DeleteImagesAsync([NotNull] int[] id, CancellationToken token = default(CancellationToken));
}