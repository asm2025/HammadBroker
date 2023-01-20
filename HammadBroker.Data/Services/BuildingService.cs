using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class BuildingService : Service<DataContext, IBuildingRepository, Building, int>, IBuildingService
{
	public BuildingService([NotNull] IBuildingRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<BuildingService> logger)
		: base(repository, mapper, logger)
	{
	}

	/// <inheritdoc />
	public IList<BuildingForList> List(BuildingList settings)
	{
		ThrowIfDisposed();

		IQueryable<Building> queryable = Repository.List(settings);
		IList<BuildingForList> result = queryable.ProjectTo<BuildingForList>(Mapper.ConfigurationProvider)
														.ToList();
		return result;
	}

	/// <inheritdoc />
	public async Task<IList<BuildingForList>> ListAsync(BuildingList settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<Building> queryable = Repository.List(settings);
		IList<BuildingForList> result = await queryable.ProjectTo<BuildingForList>(Mapper.ConfigurationProvider)
														.ToListAsync(token)
														.ConfigureAwait();
		return result;
	}

	/// <inheritdoc />
	public IList<string> ListImages(int buildingId)
	{
		ThrowIfDisposed();

		IQueryable<BuildingImage> entities = Repository.ListImages(buildingId);
		IList<string> result = entities.Select(e => e.ImageUrl)
								.ToList();
		return result;
	}

	/// <inheritdoc />
	public async Task<IList<string>> ListImagesAsync(int buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingImage> entities = Repository.ListImages(buildingId);
		IList<string> result = await entities.Select(e => e.ImageUrl)
										.ToListAsync(token)
										.ConfigureAwait();
		return result;
	}

	/// <inheritdoc />
	public T GetBuildingImage<T>(int id)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.GetImage(id);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<T> GetBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.GetImageAsync(id, token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public T GetBuildingImage<T>(int id, IGetSettings settings)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.GetImage(id, settings);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<T> GetBuildingImageAsync<T>(int id, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.GetImageAsync(id, settings, token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public void AddImage(int buildingId, string imageUrl)
	{
		ThrowIfDisposed();
		Repository.AddImage(buildingId, imageUrl);
		Context.SaveChanges();
	}

	/// <inheritdoc />
	public async Task AddImageAsync(int buildingId, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Repository.AddImageAsync(buildingId, imageUrl, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
	}

	/// <inheritdoc />
	public void AddImage(Building building, string imageUrl)
	{
		ThrowIfDisposed();
		Repository.AddImage(building, imageUrl);
		Context.SaveChanges();
	}

	/// <inheritdoc />
	public async Task AddImageAsync(Building building, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Repository.AddImageAsync(building, imageUrl, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
	}

	/// <inheritdoc />
	public void UpdateImage(int buildingId, string imageUrl)
	{
		ThrowIfDisposed();
		Repository.UpdateImage(buildingId, imageUrl);
		Context.SaveChanges();
	}

	/// <inheritdoc />
	public async Task UpdateImageAsync(int buildingId, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Repository.UpdateImageAsync(buildingId, imageUrl, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
	}

	/// <inheritdoc />
	public BuildingImage UpdateBuildingImage(int id, string imageUrl)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.UpdateImage(id, imageUrl);
		if (entity == null) return null;
		Context.SaveChanges();
		return entity;
	}

	/// <inheritdoc />
	public T UpdateBuildingImage<T>(int id, string imageUrl)
	{
		BuildingImage entity = UpdateBuildingImage(id, imageUrl);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<BuildingImage> UpdateBuildingImageAsync(int id, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.UpdateImageAsync(id, imageUrl, token);
		token.ThrowIfCancellationRequested();
		if (entity == null) return null;
		await Context.SaveChangesAsync(token);
		return entity;
	}

	/// <inheritdoc />
	public async Task<T> UpdateBuildingImageAsync<T>(int id, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		BuildingImage entity = await UpdateBuildingImageAsync(id, imageUrl, token);
		token.ThrowIfCancellationRequested();
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public BuildingImage DeleteBuildingImage(int id)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.DeleteImage(id);
		if (entity == null) return null;
		Context.SaveChanges();
		return entity;
	}

	/// <inheritdoc />
	public T DeleteBuildingImage<T>(int id)
	{
		BuildingImage entity = DeleteBuildingImage(id);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<BuildingImage> DeleteBuildingImageAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.DeleteImageAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (entity == null) return null;
		await Context.SaveChangesAsync(token);
		return entity;
	}

	/// <inheritdoc />
	public async Task<T> DeleteBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken))
	{
		BuildingImage entity = await DeleteBuildingImageAsync(id, token);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public BuildingImage DeleteBuildingImage(BuildingImage image)
	{
		ThrowIfDisposed();
		image = Repository.DeleteImage(image);
		Context.SaveChanges();
		return image;
	}

	/// <inheritdoc />
	public T DeleteBuildingImage<T>(BuildingImage image)
	{
		image = DeleteBuildingImage(image);
		return Mapper.Map<T>(image);
	}

	/// <inheritdoc />
	public async Task<BuildingImage> DeleteBuildingImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		image = await Repository.DeleteImageAsync(image, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
		return image;
	}

	/// <inheritdoc />
	public async Task<T> DeleteBuildingImageAsync<T>(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		image = await DeleteBuildingImageAsync(image, token);
		return Mapper.Map<T>(image);
	}

	protected override IQueryable<Building> PrepareListQuery(IQueryable<Building> queryable, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareListQuery(queryable, settings);
		queryable = PrepareLocation(queryable, buildingList);
		queryable = PrepareNumbers(queryable, buildingList);
		queryable = PrepareSearch(queryable, buildingList);
		return base.PrepareListQuery(queryable, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<Building> PrepareCountQuery(IQueryable<Building> queryable, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return base.PrepareCountQuery(queryable, settings);
		queryable = PrepareLocation(queryable, buildingList);
		queryable = PrepareNumbers(queryable, buildingList);
		queryable = PrepareSearch(queryable, buildingList);
		return base.PrepareCountQuery(queryable, settings);
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