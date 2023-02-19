using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
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

public class BuildingService : Service<DataContext, IBuildingRepository, Building, string>, IBuildingService
{
	public BuildingService([NotNull] IBuildingRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<BuildingService> logger)
		: base(repository, mapper, logger)
	{
	}

	/// <inheritdoc />
	public IList<T> List<T>(BuildingList settings)
	{
		ThrowIfDisposed();
		IQueryable<Building> queryable = Repository.List(settings);
		IList<T> result = queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
														.ToList();
		return result;
	}

	/// <inheritdoc />
	public async Task<IList<T>> ListAsync<T>(BuildingList settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<Building> queryable = Repository.List(settings);
		IList<T> result = await queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
														.ToListAsync(token)
														.ConfigureAwait();
		return result;
	}

	/// <inheritdoc />
	public IList<BuildingImageForList> ListImages(string buildingId)
	{
		ThrowIfDisposed();
		IList<BuildingImageForList> result = Repository.ListImages(buildingId)
														.ProjectTo<BuildingImageForList>(Mapper.ConfigurationProvider)
														.ToList();
		return result;
	}

	/// <inheritdoc />
	public Task<IList<BuildingImageForList>> ListImagesAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.ListImages(buildingId)
						.ProjectTo<BuildingImageForList>(Mapper.ConfigurationProvider)
						.ToListAsync(token)
						.As<List<BuildingImageForList>, IList<BuildingImageForList>>(token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(string buildingId)
	{
		ThrowIfDisposed();
		return Repository.GetImage(buildingId);
	}

	/// <inheritdoc />
	public Task<BuildingImage> GetImageAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.GetImageAsync(buildingId, token);
	}

	/// <inheritdoc />
	public BuildingImage GetImage(int id)
	{
		ThrowIfDisposed();
		return Repository.GetImage(id);
	}

	/// <inheritdoc />
	public ValueTask<BuildingImage> GetImageAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.GetImageAsync(id, token);
	}

	/// <inheritdoc />
	public BuildingImage AddImage(BuildingImage image)
	{
		ThrowIfDisposed();
		Repository.AddImage(image);
		Context.SaveChanges();
		return image;
	}

	/// <inheritdoc />
	public async ValueTask<BuildingImage> AddImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Repository.AddImageAsync(image, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
		return image;
	}

	/// <inheritdoc />
	public BuildingImage UpdateImage(BuildingImage image)
	{
		ThrowIfDisposed();
		Repository.UpdateImage(image);
		Context.SaveChanges();
		return image;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> UpdateImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		await Repository.UpdateImageAsync(image, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
		return image;
	}

	/// <inheritdoc />
	public BuildingImage DeleteImage(int id)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.DeleteImage(id);
		if (entity == null) return null;
		Context.SaveChanges();
		return entity;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> DeleteImageAsync(int id, CancellationToken token = default(CancellationToken))
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
	public BuildingImage DeleteImage(BuildingImage image)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.DeleteImage(image);
		Context.SaveChanges();
		return entity;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> DeleteImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage entity = await Repository.DeleteImageAsync(image, token);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
		return entity;
	}

	/// <inheritdoc />
	public void DeleteImages(string buildingId)
	{
		ThrowIfDisposed();
		Repository.DeleteImages(buildingId);
		Context.SaveChanges();
	}

	/// <inheritdoc />
	public async Task DeleteImagesAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		Repository.DeleteImages(buildingId);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);
	}
}