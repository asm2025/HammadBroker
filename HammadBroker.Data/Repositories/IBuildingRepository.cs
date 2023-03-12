using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Data.Repositories;

public interface IBuildingRepository : IRepository<DataContext, Building, int>
{
	public DbSet<BuildingImage> Images { get; }

	IDictionary<int, string> GetMainImages([NotNull] ICollection<int> ids);
	[NotNull]
	Task<IDictionary<int, string>> GetMainImagesAsync([NotNull] ICollection<int> ids, CancellationToken token = default(CancellationToken));
	BuildingImage GetMainImage(int buildingId);
	[NotNull]
	Task<BuildingImage> GetMainImageAsync(int buildingId, CancellationToken token = default(CancellationToken));
	IQueryable<BuildingImage> ListImages(int buildingId, IPagination settings = null);
	[NotNull]
	Task<IList<BuildingImage>> ListImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
	int CountImages(int buildingId, IPagination settings = null);
	[NotNull]
	Task<int> CountImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
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
	[NotNull]
	IList<BuildingImage> DeleteImages([NotNull] int[] id);
	[NotNull]
	[ItemNotNull]
	Task<IList<BuildingImage>> DeleteImagesAsync([NotNull] int[] id, CancellationToken token = default(CancellationToken));
}