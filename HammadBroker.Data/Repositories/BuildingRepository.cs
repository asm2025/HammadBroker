using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
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

public class BuildingRepository : Repository<DataContext, Building, int>, IBuildingRepository
{
	/// <inheritdoc />
	public BuildingRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingRepository> logger)
		: base(context, configuration, logger)
	{
		Images = Context.BuildingImages;
	}

	protected DbSet<BuildingImage> Images { get; }

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareCountQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareCountQuery(query, settings);
		if (buildingList.Id > 0) return query.Where(e => e.Id == buildingList.Id.Value);
		query = PrepareLocation(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareSearch(query, buildingList);
		return base.PrepareCountQuery(query, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareListQuery(IQueryable<Building> query, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareListQuery(query, settings);
		if (buildingList.Id > 0) return query.Where(e => e.Id == buildingList.Id.Value);
		query = PrepareLocation(query, buildingList);
		query = PrepareNumbers(query, buildingList);
		query = PrepareSearch(query, buildingList);
		return base.PrepareListQuery(query, settings);
	}

	/// <inheritdoc />
	protected override Building AddInternal(Building entity)
	{
		if (entity is { CityId: > 0 })
		{
			City city = Context.Cities.Find(entity.CityId);
			if (city != null && !string.Equals(entity.CountryCode, city.CountryCode, StringComparison.OrdinalIgnoreCase)) entity.CountryCode = city.CountryCode;
		}

		return base.AddInternal(entity);
	}

	/// <inheritdoc />
	protected override async ValueTask<Building> AddAsyncInternal(Building entity, CancellationToken token = new CancellationToken())
	{
		if (entity is { CityId: > 0 })
		{
			City city = await Context.Cities.FindAsync(new object[] { entity.CityId }, token);
			if (city != null && !string.Equals(entity.CountryCode, city.CountryCode, StringComparison.OrdinalIgnoreCase)) entity.CountryCode = city.CountryCode;
		}

		return await base.AddAsyncInternal(entity, token);
	}

	/// <inheritdoc />
	protected override Building UpdateInternal(Building entity)
	{
		if (entity is { CityId: > 0 })
		{
			City city = Context.Cities.Find(entity.CityId);
			if (city != null && !string.Equals(entity.CountryCode, city.CountryCode, StringComparison.OrdinalIgnoreCase)) entity.CountryCode = city.CountryCode;
		}

		return base.UpdateInternal(entity);
	}

	/// <inheritdoc />
	protected override async ValueTask<Building> UpdateAsyncInternal(Building entity, CancellationToken token = new CancellationToken())
	{
		if (entity is { CityId: > 0 })
		{
			City city = await Context.Cities.FindAsync(new object[] { entity.CityId }, token);
			if (city != null && !string.Equals(entity.CountryCode, city.CountryCode, StringComparison.OrdinalIgnoreCase)) entity.CountryCode = city.CountryCode;
		}

		return await base.UpdateAsyncInternal(entity, token);
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
		settings ??= new Pagination();
		return PrepareImageListQuery(buildingId, settings).Paginate(settings).ToListAsync(token).As<List<BuildingImage>, IList<BuildingImage>>(token);
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
		settings ??= new Pagination();
		return PrepareImageCountQuery(buildingId, settings).CountAsync(token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(int id)
	{
		ThrowIfDisposed();
		return PrepareImageGetQuery(id).FirstOrDefault();
	}

	/// <inheritdoc />
	public Task<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return PrepareImageGetQuery(id).FirstOrDefaultAsync(token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(int id, IGetSettings settings)
	{
		ThrowIfDisposed();
		return PrepareImageGetQuery(id, settings).FirstOrDefault();
	}

	/// <inheritdoc />
	public Task<BuildingImage> GetImageAsync(int id, IGetSettings settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return PrepareImageGetQuery(id, settings).FirstOrDefaultAsync(token);
	}

	/// <inheritdoc />
	public void AddImage(int buildingId, string imageUrl)
	{
		ThrowIfDisposed();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));
		Building building = DbSet.Find(buildingId);
		if (building == null) throw new ArgumentOutOfRangeException(nameof(buildingId));
		AddImage(building, imageUrl);
	}

	/// <inheritdoc />
	public async ValueTask AddImageAsync(int buildingId, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));
		Building building = await DbSet.FindAsync(new object[] { buildingId }, token);
		if (building == null) throw new ArgumentOutOfRangeException(nameof(buildingId));
		await AddImageAsync(building, imageUrl, token);
	}

	/// <inheritdoc />
	public void AddImage(Building building, string imageUrl)
	{
		ThrowIfDisposed();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));

		Images.Add(new BuildingImage
		{
			BuildingId = building.Id,
			ImageUrl = imageUrl
		});
	}

	/// <inheritdoc />
	public async ValueTask AddImageAsync(Building building, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));

		await Images.AddAsync(new BuildingImage
		{
			BuildingId = building.Id,
			ImageUrl = imageUrl
		}, token);
	}

	/// <inheritdoc />
	public BuildingImage UpdateImage(int id, string imageUrl)
	{
		ThrowIfDisposed();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));
		BuildingImage image = Images.Find(id);
		if (image == null) return null;
		image.ImageUrl = imageUrl;
		Context.Entry(image).State = EntityState.Modified;
		return image;
	}

	/// <inheritdoc />
	public async ValueTask<BuildingImage> UpdateImageAsync(int id, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));
		BuildingImage image = await Images.FindAsync(new object[] { id }, token);
		if (image == null) return null;
		image.ImageUrl = imageUrl;
		Context.Entry(image).State = EntityState.Modified;
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
	public async ValueTask<BuildingImage> DeleteImageAsync(int id, CancellationToken token = default(CancellationToken))
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
	public ValueTask<BuildingImage> DeleteImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return new ValueTask<BuildingImage>(DeleteImageInternal(image));
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
	private IQueryable<BuildingImage> PrepareImageGetQuery(int id)
	{
		IQueryable<BuildingImage> query = Images;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({KeyProperty.Name} == ");

		if (KeyProperty.PropertyType.IsNumeric())
			filter.Append(id);
		else
			filter.Append($"\"{id}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	[NotNull]
	private IQueryable<BuildingImage> PrepareImageGetQuery(int id, IGetSettings settings)
	{
		IQueryable<BuildingImage> query = PrepareImageGetQuery(id);

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

	[NotNull]
	private BuildingImage DeleteImageInternal([NotNull] BuildingImage image)
	{
		if (Context.Entry(image).State == EntityState.Detached) Images.Attach(image);
		return Images.Remove(image).Entity;
	}

	[NotNull]
	private static IQueryable<Building> PrepareLocation([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (!string.IsNullOrEmpty(buildingList.CountryCode)) queryable = queryable.Where(e => e.CountryCode == buildingList.CountryCode);
		if (buildingList.CityId > 0) queryable = queryable.Where(e => e.CityId == buildingList.CityId);
		return queryable;
	}

	[NotNull]
	private static IQueryable<Building> PrepareNumbers([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList.BuildingType.HasValue) queryable = queryable.Where(e => e.BuildingType == buildingList.BuildingType.Value);
		if (buildingList.FinishingType.HasValue) queryable = queryable.Where(e => e.FinishingType == buildingList.FinishingType.Value);

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
	private static IQueryable<Building> PrepareSearch([NotNull] IQueryable<Building> queryable, [NotNull] BuildingList buildingList)
	{
		if (!string.IsNullOrEmpty(buildingList.Search)) queryable = queryable.Where(e => e.Name.Contains(buildingList.Search));
		if (!string.IsNullOrEmpty(buildingList.Address)) queryable = queryable.Where(e => e.Address.Contains(buildingList.Address) || e.Address2.Contains(buildingList.Address));
		return queryable;
	}
}