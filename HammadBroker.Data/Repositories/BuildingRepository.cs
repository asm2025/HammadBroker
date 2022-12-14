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
		Building building = await DbSet.FindAsync(new[] { buildingId }, token);
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
		BuildingImage image = await Images.FindAsync(new[] { id }, token);
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
		BuildingImage image = await Images.FindAsync(new[] { id }, token);
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
}