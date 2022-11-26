using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Repositories;

public interface IBuildingRepository : IRepository<DataContext, Building, int>
{
	IQueryable<BuildingImage> ListImages(int buildingId, IPagination settings = null);
	[NotNull]
	Task<IList<BuildingImage>> ListImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
	int CountImages(int buildingId, IPagination settings = null);
	[NotNull]
	Task<int> CountImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage(int id);
	[NotNull]
	Task<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken));
	BuildingImage GetImage(int id, IGetSettings settings);
	[NotNull]
	Task<BuildingImage> GetImageAsync(int id, IGetSettings settings = null, CancellationToken token = default(CancellationToken));
	void AddImage(int buildingId, [NotNull] string imageUrl);
	ValueTask AddImageAsync(int buildingId, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	void AddImage([NotNull] Building building, [NotNull] string imageUrl);
	ValueTask AddImageAsync([NotNull] Building building, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	void UpdateImage(int buildingId, [NotNull] string imageUrl);
	ValueTask UpdateImageAsync(int buildingId, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage UpdateBuildingImage(int id, [NotNull] string imageUrl);
	[ItemNotNull]
	ValueTask<BuildingImage> UpdateBuildingImageAsync(int id, [NotNull] string imageUrl, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage DeleteImage(int id);
	[ItemNotNull]
	ValueTask<BuildingImage> DeleteImageAsync(int id, CancellationToken token = default(CancellationToken));
	[NotNull]
	BuildingImage DeleteImage([NotNull] BuildingImage image);
	[ItemNotNull]
	ValueTask<BuildingImage> DeleteImageAsync([NotNull] BuildingImage image, CancellationToken token = default(CancellationToken));
}