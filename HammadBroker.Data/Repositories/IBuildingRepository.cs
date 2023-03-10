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

public interface IBuildingRepository : IRepository<DataContext, Building, string>
{
	public DbSet<BuildingImage> Images { get; }

	IDictionary<string, string> GetMainImages([NotNull] ICollection<string> ids);
	[NotNull]
	Task<IDictionary<string, string>> GetMainImagesAsync([NotNull] ICollection<string> ids, CancellationToken token = default(CancellationToken));
	string GetMainImage([NotNull] string buildingId);
	[NotNull]
	Task<string> GetMainImageAsync([NotNull] string buildingId, CancellationToken token = default(CancellationToken));
	IQueryable<BuildingImage> ListImages([NotNull] string buildingId, IPagination settings = null);
	[NotNull]
	Task<IList<BuildingImage>> ListImagesAsync([NotNull] string buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
	int CountImages([NotNull] string buildingId, IPagination settings = null);
	[NotNull]
	Task<int> CountImagesAsync([NotNull] string buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken));
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
}