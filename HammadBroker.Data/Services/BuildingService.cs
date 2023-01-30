using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
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
}