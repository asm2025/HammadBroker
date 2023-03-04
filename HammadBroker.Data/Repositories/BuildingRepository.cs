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
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class BuildingRepository : Repository<DataContext, Building, string>, IBuildingRepository
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
	protected override Building AddInternal(Building entity)
	{
		if (entity == null || string.IsNullOrEmpty(entity.VideoId)) return base.AddInternal(entity);
		Match match = __youtubeId.Match(entity.VideoId);
		entity.VideoId = match.Success ? match.Groups["id"].Value : null;
		return base.AddInternal(entity);
	}

	/// <inheritdoc />
	protected override Building UpdateInternal(Building entity)
	{
		if (entity == null || string.IsNullOrEmpty(entity.VideoId)) return base.UpdateInternal(entity);
		Match match = __youtubeId.Match(entity.VideoId);
		string videoId = match.Success ? match.Groups["id"].Value : null;
		if (videoId == entity.VideoId) return base.UpdateInternal(entity);
		entity.VideoId = videoId;
		Context.Entry(entity).State = EntityState.Modified;
		return base.UpdateInternal(entity);
	}

	public IDictionary<string, string> GetMainImages(ICollection<string> ids)
	{
		if (ids.Count == 0) return null;
		return Images.Where(e => ids.Contains(e.BuildingId))
					.DefaultIfEmpty()
					.GroupBy(e => e.BuildingId)
					.Select(g => g.OrderByDescending(e => e.Priority ?? 0).Take(1))
					.SelectMany(g => g)
					.ToDictionary(k => k.BuildingId, v => v.ImageUrl, StringComparer.OrdinalIgnoreCase);
	}

	public Task<IDictionary<string, string>> GetMainImagesAsync(ICollection<string> ids, CancellationToken token = default(CancellationToken))
	{
		if (ids.Count == 0) return Task.FromResult<IDictionary<string, string>>(null);
		return Images.Where(e => ids.Contains(e.BuildingId))
					.DefaultIfEmpty()
					.GroupBy(e => e.BuildingId)
					.Select(g => g.OrderByDescending(e => e.Priority ?? 0).Take(1))
					.SelectMany(g => g)
					.ToDictionaryAsync(k => k.BuildingId, v => v.ImageUrl, StringComparer.OrdinalIgnoreCase, token)
					.As<Dictionary<string, string>, IDictionary<string, string>>(token);
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareCountQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareCountQuery(query, settings);
		if (!string.IsNullOrEmpty(buildingList.Id)) return query.Where(e => e.Id.Contains(buildingList.Id));
		query = PrepareTypes(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareDateAndLocation(query, buildingList);
		return base.PrepareCountQuery(query, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareListQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareListQuery(query, settings);
		if (!string.IsNullOrEmpty(buildingList.Id)) return query.Where(e => e.Id.Contains(buildingList.Id));
		query = PrepareTypes(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareDateAndLocation(query, buildingList);
		return base.PrepareListQuery(query, settings);
	}

	/// <inheritdoc />
	public IQueryable<BuildingImage> ListImages(string buildingId, IPagination settings = null)
	{
		ThrowIfDisposed();
		return PrepareImageListQuery(buildingId, settings);
	}

	/// <inheritdoc />
	public Task<IList<BuildingImage>> ListImagesAsync(string buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken))
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
	public int CountImages(string buildingId, IPagination settings = null)
	{
		ThrowIfDisposed();
		return PrepareImageCountQuery(buildingId, settings).Count();
	}

	/// <inheritdoc />
	public Task<int> CountImagesAsync(string buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return PrepareImageCountQuery(buildingId, settings).CountAsync(token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(string buildingId)
	{
		ThrowIfDisposed();
		return Images.Where(e => e.BuildingId == buildingId)
					.OrderByDescending(e => e.Priority)
					.FirstOrDefault();
	}

	/// <inheritdoc />
	public Task<BuildingImage> GetImageAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Images.Where(e => e.BuildingId == buildingId)
					.OrderByDescending(e => e.Priority)
					.FirstOrDefaultAsync(token);
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

	private IQueryable<BuildingImage> PrepareImageCountQuery([NotNull] string buildingId, IPagination settings) { return PrepareImageCountQuery(Images.Where(e => e.BuildingId == buildingId), settings); }
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

	private IQueryable<BuildingImage> PrepareImageListQuery([NotNull] string buildingId, IPagination settings) { return PrepareImageListQuery(Images.Where(e => e.BuildingId == buildingId), settings); }
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

		if (buildingList.CityId > 0) queryable = queryable.Where(e => e.CityId == buildingList.CityId);
		if (!string.IsNullOrEmpty(buildingList.Address)) queryable = queryable.Where(e => e.Address.Contains(buildingList.Address) || e.Address2.Contains(buildingList.Address));
		return queryable;
	}
}