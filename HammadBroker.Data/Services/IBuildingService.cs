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

public interface IBuildingService : IService<DataContext, IBuildingRepository, Building, int>
{
	[NotNull]
	BuildingsPaginated<T> List<T>(BuildingList settings = null);

	[NotNull]
	[ItemNotNull]
	Task<BuildingsPaginated<T>> ListAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<T> Lookup<T>(BuildingList settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IList<T>> LookupAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<BuildingImage> ListImages(int buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImage>> ListImagesAsync(int buildingId, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<T> ListImages<T>(int buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<T>> ListImagesAsync<T>(int buildingId, CancellationToken token = default(CancellationToken));
	BuildingImage GetMainImage(int buildingId);
	[NotNull]
	Task<BuildingImage> GetMainImageAsync(int buildingId, CancellationToken token = default(CancellationToken));
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
	void DeleteImages(int buildingId);
	[NotNull]
	Task DeleteImagesAsync(int buildingId, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<BuildingImage> DeleteImages([NotNull] int[] id);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImage>> DeleteImagesAsync([NotNull] int[] id, CancellationToken token = default(CancellationToken));
}