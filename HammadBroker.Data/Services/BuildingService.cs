using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using HammadBroker.Model.VirtualPath;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class BuildingService : Service<DataContext, IBuildingRepository, Building, string>, IBuildingService
{
	private readonly string _assetImagesPath;

	public BuildingService([NotNull] IBuildingRepository repository, [NotNull] IMapper mapper, [NotNull] VirtualPathSettings virtualPathSettings, [NotNull] IWebHostEnvironment environment, [NotNull] ILogger<BuildingService> logger)
		: base(repository, mapper, logger)
	{
		PathContent assetsPath = virtualPathSettings.PathContents?.FirstOrDefault(e => e.Alias.IsSame("AssetImages")) ?? throw new Exception($"{nameof(VirtualPathSettings)} does not contain a definition for AssetImages.");
		_assetImagesPath = Path.Combine(environment.WebRootPath, assetsPath.PhysicalPath).Suffix(Path.DirectorySeparatorChar);
	}

	/// <inheritdoc />
	public BuildingsPaginated<T> List<T>(BuildingList settings = null)
		where T : class, IBuildingLookup
	{
		ThrowIfDisposed();
		settings ??= new BuildingList();
		IQueryable<Building> queryable = Repository.List(settings);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = PrepareCountQuery(queryable, settings).Count();
			int maxPage = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPage) settings.Page = maxPage;
		}

		IList<T> result = queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
									.ToList();
		if (result.Count == 0) return new BuildingsPaginated<T>(result, settings);

		ICollection<string> ids = result.Select(e => e.Id).ToList();
		IDictionary<string, string> images = Repository.GetMainImages(ids);
		if (images == null) return new BuildingsPaginated<T>(result, settings);

		foreach (T building in result)
		{
			if (!images.TryGetValue(building.Id, out string imageUrl)) continue;
			building.ImageUrl = imageUrl;
		}

		return new BuildingsPaginated<T>(result, settings);
	}

	/// <inheritdoc />
	public async Task<BuildingsPaginated<T>> ListAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken))
		where T : class, IBuildingLookup
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		settings ??= new BuildingList();
		IQueryable<Building> queryable = Repository.List(settings);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await PrepareCountQuery(queryable, settings).CountAsync(token);
			token.ThrowIfCancellationRequested();
			int maxPage = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPage) settings.Page = maxPage;
		}

		IList<T> result = await queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
									.ToListAsync(token);
		if (result.Count == 0) return new BuildingsPaginated<T>(result, settings);

		ICollection<string> ids = result.Select(e => e.Id).ToList();
		IDictionary<string, string> images = await Repository.GetMainImagesAsync(ids, token);
		token.ThrowIfCancellationRequested();
		if (images == null) return new BuildingsPaginated<T>(result, settings);

		foreach (T building in result)
		{
			if (!images.TryGetValue(building.Id, out string imageUrl)) continue;
			building.ImageUrl = imageUrl;
		}

		return new BuildingsPaginated<T>(result, settings);
	}

	/// <inheritdoc />
	public IList<T> Lookup<T>(BuildingList settings = null)
	{
		ThrowIfDisposed();
		IQueryable<Building> queryable = Repository.List(settings);
		IList<T> result = queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
														.ToList();
		return result;
	}

	/// <inheritdoc />
	public async Task<IList<T>> LookupAsync<T>(BuildingList settings = null, CancellationToken token = default(CancellationToken))
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
		BuildingImage oldImage = Repository.GetImage(image.Id);
		if (oldImage == null) return null;
		if (!oldImage.ImageUrl.IsSame(image.ImageUrl)) FileHelper.Delete(GetImagePath(oldImage.ImageUrl));
		Repository.UpdateImage(image);
		Context.SaveChanges();
		return image;
	}

	/// <inheritdoc />
	public async Task<BuildingImage> UpdateImageAsync(BuildingImage image, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		BuildingImage oldImage = await Repository.GetImageAsync(image.Id, token);
		if (oldImage == null) return null;
		if (!oldImage.ImageUrl.IsSame(image.ImageUrl)) FileHelper.Delete(GetImagePath(oldImage.ImageUrl));
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
		FileHelper.Delete(GetImagePath(entity.ImageUrl));
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
		FileHelper.Delete(GetImagePath(entity.ImageUrl));
		return entity;
	}

	/// <inheritdoc />
	public BuildingImage DeleteImage(BuildingImage image)
	{
		ThrowIfDisposed();
		BuildingImage entity = Repository.DeleteImage(image);
		Context.SaveChanges();
		FileHelper.Delete(GetImagePath(entity.ImageUrl));
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
		FileHelper.Delete(GetImagePath(entity.ImageUrl));
		return entity;
	}

	/// <inheritdoc />
	public void DeleteImages(string buildingId)
	{
		ThrowIfDisposed();
		IQueryable<BuildingImage> queryable = Repository.Images.Where(e => e.BuildingId == buildingId);
		IList<string> fileNames = queryable.Select(e => e.ImageUrl).ToList();
		Repository.Images.RemoveRange(queryable);
		Context.SaveChanges();

		foreach (string fileName in fileNames)
		{
			FileHelper.Delete(GetImagePath(fileName));
		}
	}

	/// <inheritdoc />
	public async Task DeleteImagesAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<BuildingImage> queryable = Repository.Images.Where(e => e.BuildingId == buildingId);
		IList<string> fileNames = await queryable.Select(e => e.ImageUrl).ToListAsync(token);
		token.ThrowIfCancellationRequested();
		Repository.Images.RemoveRange(queryable);
		token.ThrowIfCancellationRequested();
		await Context.SaveChangesAsync(token);

		foreach (string fileName in fileNames)
		{
			FileHelper.Delete(GetImagePath(fileName));
		}
	}

	private string GetImagePath(string fileName)
	{
		return string.IsNullOrEmpty(fileName)
					? null
					: Path.Combine(_assetImagesPath, fileName);
	}
}