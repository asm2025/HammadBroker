using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
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
	public override IPaginated<T> List<T>(IQueryable<Building> queryable, IPagination settings = null)
	{
		ThrowIfDisposed();
		return base.List<T>(PrepareList(queryable, settings), settings);
	}

	/// <inheritdoc />
	public override Task<IPaginated<T>> ListAsync<T>(IQueryable<Building> queryable, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return base.ListAsync<T>(PrepareList(queryable, settings), settings, token);
	}

	/// <inheritdoc />
	public IPaginated<string> ListImages(int buildingId, IPagination settings = null)
	{
		ThrowIfDisposed();

		IQueryable<BuildingImage> entities = Repository.ListImages(buildingId, settings);

		if (settings is { PageSize: > 0 })
		{
			entities = entities.Paginate(settings);
			settings.Count = Repository.Count(settings);
		}

		IList<string> result = entities.Select(e => e.ImageUrl)
								.ToList();
		return new Paginated<string>(result, settings);
	}

	/// <inheritdoc />
	public async Task<IPaginated<string>> ListImagesAsync(int buildingId, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingImage> entities = Repository.ListImages(buildingId, settings);

		if (settings is { PageSize: > 0 })
		{
			entities = entities.Paginate(settings);
			settings.Count = await Repository.CountAsync(settings, token);
			token.ThrowIfCancellationRequested();
		}

		IList<string> result = await entities.Select(e => e.ImageUrl)
										.ToListAsync(token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new Paginated<string>(result, settings);
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
	public T UpdateBuildingImage<T>(int id, string imageUrl)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.UpdateBuildingImage(id, imageUrl);
		Context.SaveChanges();
		return Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<T> UpdateBuildingImageAsync<T>(int id, string imageUrl, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.UpdateBuildingImageAsync(id, imageUrl, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
		return Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public T DeleteBuildingImage<T>(int id)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.DeleteImage(id);
		Context.SaveChanges();
		return Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public async Task<T> DeleteBuildingImageAsync<T>(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.DeleteImageAsync(id, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token)
					.ConfigureAwait();
		return Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public T DeleteBuildingImage<T>(BuildingImage image)
	{
		ThrowIfDisposed();
		image = Repository.DeleteImage(image);
		Context.SaveChanges();
		return Mapper.Map<T>(image);
	}

	/// <inheritdoc />
	public async Task<T> DeleteBuildingImageAsync<T>(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		image = await Repository.DeleteImageAsync(image, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token)
					.ConfigureAwait();
		return Mapper.Map<T>(image);
	}

	[NotNull]
	private static IQueryable<Building> PrepareList([NotNull] IQueryable<Building> queryable, IPagination settings)
	{
		if (settings is not BuildingList buildingList) return queryable;
		if (buildingList.CityId.HasValue) queryable = queryable.Where(e => e.CityId == buildingList.CityId.Value);
		queryable = PrepareNumbers(queryable, buildingList);
		return PrepareSearch(queryable, buildingList);

		[NotNull]
		static IQueryable<Building> PrepareNumbers([NotNull] IQueryable<Building> queryable, BuildingList buildingList)
		{
			if (buildingList.BuildingType.HasValue) queryable = queryable.Where(e => e.BuildingType == buildingList.BuildingType.Value);
			if (buildingList.FinishingType.HasValue) queryable = queryable.Where(e => e.FinishingType == buildingList.FinishingType.Value);
			if (buildingList.Floor.HasValue) queryable = queryable.Where(e => e.Floor == buildingList.Floor.Value);

			if (buildingList.Rooms.HasValue)
			{
				if (buildingList.MaxRooms.HasValue && buildingList.MaxRooms > buildingList.Rooms)
					queryable = queryable.Where(e => e.Rooms >= buildingList.Rooms.Value && e.Rooms <= buildingList.MaxRooms.Value);
				else
					queryable = queryable.Where(e => e.Rooms == buildingList.Rooms.Value);
			}

			if (buildingList.Bathrooms.HasValue)
			{
				if (buildingList.MaxBathrooms.HasValue && buildingList.MaxBathrooms > buildingList.Bathrooms)
					queryable = queryable.Where(e => e.Bathrooms >= buildingList.Bathrooms.Value && e.Bathrooms <= buildingList.MaxBathrooms.Value);
				else
					queryable = queryable.Where(e => e.Bathrooms == buildingList.Bathrooms.Value);
			}

			if (buildingList.Area.HasValue)
			{
				if (buildingList.MaxArea.HasValue && buildingList.MaxArea > buildingList.Area)
					queryable = queryable.Where(e => e.Area >= buildingList.Area.Value && e.Area <= buildingList.MaxArea.Value);
				else
					queryable = queryable.Where(e => e.Area == buildingList.Area.Value);
			}

			return queryable;
		}

		[NotNull]
		static IQueryable<Building> PrepareSearch([NotNull] IQueryable<Building> queryable, BuildingList buildingList)
		{
			if (!string.IsNullOrEmpty(buildingList.Search)) queryable = queryable.Where(e => e.Name.Contains(buildingList.Search));
			if (!string.IsNullOrEmpty(buildingList.Address)) queryable = queryable.Where(e => e.Address.Contains(buildingList.Address) || e.Address2.Contains(buildingList.Address));
			return queryable;
		}
	}
}