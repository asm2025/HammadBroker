using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface IBuildingService : IService<DataContext, IBuildingRepository, Building, int>
{
	[NotNull]
	IPaginated<string> ListImages(int buildingId, IPagination settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IPaginated<string>> ListImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
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
	T UpdateBuildingImage<T>(int id, [NotNull] string imageUrl);
	Task<T> UpdateBuildingImageAsync<T>(int id, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	T DeleteBuildingImage<T>(int id);
	Task<T> DeleteBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken));
	T DeleteBuildingImage<T>([NotNull] BuildingImage image);
	[NotNull]
	Task<T> DeleteBuildingImageAsync<T>([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
}