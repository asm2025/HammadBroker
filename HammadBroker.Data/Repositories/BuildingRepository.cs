using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using HammadBroker.Data.Context;
using HammadBroker.Model;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StringHelper = essentialMix.Helpers.StringHelper;

namespace HammadBroker.Data.Repositories;

public class BuildingRepository : Repository<DataContext, Building, int>, IBuildingRepository
{
	private static readonly Regex __youtubeId = new Regex(@"(?<=v=|v\/|vi=|vi\/|embed\/|youtu.be\/)?(?<id>[a-zA-Z0-9_-]{11})", RegexHelper.OPTIONS_I);

	/// <inheritdoc />
	public BuildingRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingRepository> logger)
		: base(context, configuration, logger)
	{
		Images = Context.BuildingImages;
	}

	public DbSet<BuildingImage> Images { get; }

	/// <inheritdoc />
	protected override Building AddInternal([NotNull] Building entity)
	{
		if (string.IsNullOrEmpty(entity.Reference)) entity.Reference = StringHelper.RandomKey(Constants.Buildings.IdentifierLength);

		if (!string.IsNullOrEmpty(entity.VideoId))
		{
			Match match = __youtubeId.Match(entity.VideoId);
			entity.VideoId = match.Success
								? match.Groups["id"].Value
								: null;
		}

		entity.CreatedOn = DateTime.UtcNow;
		entity.UpdatedOn = entity.CreatedOn;
		return base.AddInternal(entity);
	}

	/// <inheritdoc />
	protected override Building UpdateInternal([NotNull] Building entity)
	{
		if (string.IsNullOrEmpty(entity.Reference)) entity.Reference = StringHelper.RandomKey(Constants.Buildings.IdentifierLength);

		if (!string.IsNullOrEmpty(entity.VideoId))
		{
			Match match = __youtubeId.Match(entity.VideoId);
			entity.VideoId = match.Success
								? match.Groups["id"].Value
								: null;
		}

		if (entity.CreatedOn == DateTime.MinValue) entity.CreatedOn = DateTime.UtcNow;
		entity.UpdatedOn = DateTime.UtcNow;
		Context.Entry(entity).State = EntityState.Modified;
		return base.UpdateInternal(entity);
	}

	public IDictionary<int, string> GetMainImages(ICollection<int> ids)
	{
		if (ids.Count == 0) return null;
		return Images.Where(e => ids.Contains(e.BuildingId))
					.DefaultIfEmpty()
					.GroupBy(e => e.BuildingId)
					.Select(g => g.OrderByDescending(e => e.Priority ?? 0).FirstOrDefault())
					.Where(e => e != null)
					.ToDictionary(k => k.BuildingId, v => v.ImageUrl);
	}

	public Task<IDictionary<int, string>> GetMainImagesAsync(ICollection<int> ids, CancellationToken token = default(CancellationToken))
	{
		if (ids.Count == 0) return Task.FromResult<IDictionary<int, string>>(null);
		return Images.Where(e => ids.Contains(e.BuildingId))
					.GroupBy(e => e.BuildingId)
					.Select(g => g.OrderByDescending(e => e.Priority ?? 0).FirstOrDefault())
					.ToDictionaryAsync(k => k.BuildingId, v => v.ImageUrl, token)
					.As<Dictionary<int, string>, IDictionary<int, string>>(token);
	}

	/// <inheritdoc />
	public BuildingImage GetMainImage(int buildingId)
	{
		BuildingImage image = Images.Where(e => e.BuildingId == buildingId)
									.DefaultIfEmpty()
									.OrderByDescending(e => e.Priority ?? 0)
									.FirstOrDefault(e => e != null);
		return image;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> GetMainImageAsync(int buildingId, CancellationToken token = default(CancellationToken))
	{
		BuildingImage image = await Images.Where(e => e.BuildingId == buildingId)
									.DefaultIfEmpty()
									.OrderByDescending(e => e.Priority ?? 0)
									.FirstOrDefaultAsync(e => e != null, token);
		return image;
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareCountQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareCountQuery(query, settings);
		if (!string.IsNullOrEmpty(buildingList.Reference)) return query.Where(e => e.Reference.Contains(buildingList.Reference));
		query = PrepareTypes(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareDateAndLocation(query, buildingList);
		return base.PrepareCountQuery(query, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareListQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareListQuery(query, settings);
		if (!string.IsNullOrEmpty(buildingList.Reference)) return query.Where(e => e.Reference.Contains(buildingList.Reference));
		query = PrepareTypes(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareDateAndLocation(query, buildingList);
		return base.PrepareListQuery(query, settings);
	}

	/// <inheritdoc />
	public IQueryable<BuildingImage> ListImages(int buildingId, IPagination settings = null)
	{
		ThrowIfDisposed();
		return PrepareImageListQuery(buildingId, settings);
	}

	/// <inheritdoc />
	public Task<IList<BuildingImage>> ListImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<BuildingImage> queryable = ListImages(buildingId, settings)
			.OrderByDescending(e => e.Priority);
		if (settings is { PageSize: > 0 }) queryable = queryable.Paginate(settings);
		return queryable.ToListAsync(token)
						.As<List<BuildingImage>, IList<BuildingImage>>(token);
	}

	/// <inheritdoc />
	public int CountImages(int buildingId, IPagination settings = null)
	{
		ThrowIfDisposed();
		return PrepareImageCountQuery(buildingId, settings).Count();
	}

	/// <inheritdoc />
	public Task<int> CountImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return PrepareImageCountQuery(buildingId, settings).CountAsync(token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(int id)
	{
		ThrowIfDisposed();
		return Images.Find(id);
	}

	/// <inheritdoc />
	public ValueTask<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Images.FindAsync(new object[] { id }, token);
	}

	/// <inheritdoc />
	public BuildingImage AddImage(BuildingImage image)
	{
		ThrowIfDisposed();
		Images.Add(image);
		return image;
	}

	/// <inheritdoc />
	public async ValueTask<BuildingImage> AddImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Images.AddAsync(image, token);
		return image;
	}

	/// <inheritdoc />
	public BuildingImage UpdateImage(BuildingImage image)
	{
		ThrowIfDisposed();

		if (Context.Entry(image).State == EntityState.Detached) Images.Add(image);
		else Context.Entry(image).State = EntityState.Modified;

		return image;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> UpdateImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		if (Context.Entry(image).State == EntityState.Detached) await Images.AddAsync(image, token);
		else Context.Entry(image).State = EntityState.Modified;

		return image;
	}

	/// <inheritdoc />
	public BuildingImage DeleteImage(int id)
	{
		ThrowIfDisposed();
		BuildingImage image = Images.Find(id);
		return image == null
					? null
					: DeleteImageInternal(image);
	}

	/// <inheritdoc />
	public async Task<BuildingImage> DeleteImageAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage image = await Images.FindAsync(new object[] { id }, token);
		return image == null
					? null
					: DeleteImageInternal(image);
	}

	/// <inheritdoc />
	public BuildingImage DeleteImage(BuildingImage image)
	{
		ThrowIfDisposed();
		return DeleteImageInternal(image);
	}

	/// <inheritdoc />
	public Task<BuildingImage> DeleteImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		image = DeleteImageInternal(image);
		return Task.FromResult(image);
	}

	/// <inheritdoc />
	public IList<BuildingImage> DeleteImages(int[] id)
	{
		ThrowIfDisposed();
		if (id.Length == 0) throw new ArgumentException("Image id is required.", nameof(id));

		IList<BuildingImage> images = Images.Where(e => id.Contains(e.Id))
													.ToList();
		if (images.Count > 0) Images.RemoveRange(images);
		return images;
	}

	/// <inheritdoc />
	public async Task<IList<BuildingImage>> DeleteImagesAsync(int[] id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		if (id.Length == 0) throw new ArgumentException("Image id is required.", nameof(id));

		IList<BuildingImage> images = await Images.Where(e => id.Contains(e.Id))
													.ToListAsync(token);
		token.ThrowIfCancellationRequested();
		if (images.Count > 0) Images.RemoveRange(images);
		return images;
	}

	private IQueryable<BuildingImage> PrepareImageCountQuery(int buildingId, IPagination settings) { return PrepareImageCountQuery(Images.Where(e => e.BuildingId == buildingId), settings); }
	private static IQueryable<BuildingImage> PrepareImageCountQuery(IQueryable<BuildingImage> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}

	private IQueryable<BuildingImage> PrepareImageListQuery(int buildingId, IPagination settings) { return PrepareImageListQuery(Images.Where(e => e.BuildingId == buildingId), settings); }
	private static IQueryable<BuildingImage> PrepareImageListQuery(IQueryable<BuildingImage> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	[NotNull]
	private BuildingImage DeleteImageInternal([NotNull] BuildingImage image)
	{
		if (Context.Entry(image).State == EntityState.Detached) Images.Attach(image);
		return Images.Remove(image).Entity;
	}

	[NotNull]
	private static IQueryable<Building> PrepareTypes([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList.AdType.HasValue) queryable = queryable.Where(e => e.AdType == buildingList.AdType.Value);
		if (buildingList.BuildingType.HasValue) queryable = queryable.Where(e => e.BuildingType == buildingList.BuildingType.Value);
		if (buildingList.FinishingType.HasValue) queryable = queryable.Where(e => e.FinishingType == buildingList.FinishingType.Value);
		return queryable;
	}

	[NotNull]
	private static IQueryable<Building> PrepareNumbers([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList.Price.HasValue)
		{
			queryable = buildingList.MaxPrice.HasValue
							? queryable.Where(e => e.Price >= buildingList.Price.Value && e.Price <= buildingList.MaxPrice.Value)
							: queryable.Where(e => e.Price == buildingList.Price.Value);
		}
		else if (buildingList.Price.HasValue)
		{
			queryable = queryable.Where(e => e.Price <= buildingList.MaxPrice.Value);
		}

		if (buildingList.Floor.HasValue)
		{
			queryable = buildingList.MaxFloor.HasValue
							? queryable.Where(e => e.Floor >= buildingList.Floor.Value && e.Floor <= buildingList.MaxFloor.Value)
							: queryable.Where(e => e.Floor == buildingList.Floor.Value);
		}
		else if (buildingList.MaxFloor.HasValue)
		{
			queryable = queryable.Where(e => e.Floor <= buildingList.MaxFloor.Value);
		}

		if (buildingList.Rooms.HasValue)
		{
			queryable = buildingList.MaxRooms.HasValue
							? queryable.Where(e => e.Rooms >= buildingList.Rooms.Value && e.Rooms <= buildingList.MaxRooms.Value)
							: queryable.Where(e => e.Rooms == buildingList.Rooms.Value);
		}
		else if (buildingList.MaxRooms.HasValue)
		{
			queryable = queryable.Where(e => e.Rooms <= buildingList.MaxRooms.Value);
		}

		if (buildingList.Bathrooms.HasValue)
		{
			queryable = buildingList.MaxBathrooms.HasValue
							? queryable.Where(e => e.Bathrooms >= buildingList.Bathrooms.Value && e.Bathrooms <= buildingList.MaxBathrooms.Value)
							: queryable.Where(e => e.Bathrooms == buildingList.Bathrooms.Value);
		}
		else if (buildingList.MaxBathrooms.HasValue)
		{
			queryable = queryable.Where(e => e.Bathrooms <= buildingList.MaxBathrooms.Value);
		}

		if (buildingList.Area.HasValue)
		{
			queryable = buildingList.MaxArea.HasValue
							? queryable.Where(e => e.Area >= buildingList.Area.Value && e.Area <= buildingList.MaxArea.Value)
							: queryable.Where(e => e.Area == buildingList.Area.Value);
		}
		else if (buildingList.MaxArea.HasValue)
		{
			queryable = queryable.Where(e => e.Area <= buildingList.MaxArea.Value);
		}

		return queryable;
	}

	[NotNull]
	private static IQueryable<Building> PrepareDateAndLocation([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList.Date.HasValue)
		{
			queryable = buildingList.MaxDate.HasValue
							? queryable.Where(e => e.Date >= buildingList.Date.Value && e.Date <= buildingList.MaxDate.Value)
							: queryable.Where(e => e.Date == buildingList.Date.Value);
		}
		else if (buildingList.MaxDate.HasValue)
		{
			queryable = queryable.Where(e => e.Date <= buildingList.MaxDate.Value);
		}

		if (buildingList.DistrictId > 0) queryable = queryable.Where(e => e.DistrictId == buildingList.DistrictId);
		if (buildingList.CityId > 0) queryable = queryable.Where(e => e.CityId == buildingList.CityId);
		if (!string.IsNullOrEmpty(buildingList.Address)) queryable = queryable.Where(e => e.Address.Contains(buildingList.Address));
		return queryable;
	}
}