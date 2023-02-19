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
	IList<T> List<T>(BuildingList settings);
	[NotNull]
	[ItemNotNull]
	Task<IList<T>> ListAsync<T>(BuildingList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<BuildingImageForList> ListImages([NotNull] string buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImageForList>> ListImagesAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage([NotNull] string buildingId);
	[NotNull]
	Task<BuildingImage> GetImageAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage(int id);
	ValueTask<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage AddImage([NotNull] BuildingImage image);
	[ItemNotNull]
	ValueTask<BuildingImage> AddImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage UpdateImage([NotNull] BuildingImage image);
	[NotNull]
	[ItemNotNull]
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
}