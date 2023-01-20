using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Data.Patterns.Parameters;
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
	IList<BuildingForList> List(BuildingList settings);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingForList>> ListAsync(BuildingList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	IList<string> ListImages(int buildingId);
	[NotNull]
	[ItemNotNull]
	Task<IList<string>> ListImagesAsync(int buildingId, CancellationToken token = default(CancellationToken));
	T GetBuildingImage<T>(int id);
	[NotNull]
	Task<T> GetBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken));
	T GetBuildingImage<T>(int id, [NotNull] IGetSettings settings);
	[NotNull]
	Task<T> GetBuildingImageAsync<T>(int id, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	void AddImage(int buildingId, [NotNull] string imageUrl);
	[NotNull]
	Task AddImageAsync(int buildingId, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	void AddImage([NotNull] Building building, [NotNull] string imageUrl);
	[NotNull]
	Task AddImageAsync([NotNull] Building building, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	void UpdateImage(int buildingId, [NotNull] string imageUrl);
	[NotNull]
	Task UpdateImageAsync(int buildingId, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	BuildingImage UpdateBuildingImage(int id, [NotNull] string imageUrl);
	T UpdateBuildingImage<T>(int id, [NotNull] string imageUrl);
	[NotNull]
	Task<BuildingImage> UpdateBuildingImageAsync(int id, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	Task<T> UpdateBuildingImageAsync<T>(int id, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	BuildingImage DeleteBuildingImage(int id);
	T DeleteBuildingImage<T>(int id);
	[NotNull]
	Task<BuildingImage> DeleteBuildingImageAsync(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> DeleteBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage DeleteBuildingImage([NotNull] BuildingImage image);
	T DeleteBuildingImage<T>([NotNull] BuildingImage image);
	[NotNull]
	[ItemNotNull]
	Task<BuildingImage> DeleteBuildingImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> DeleteBuildingImageAsync<T>([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
}