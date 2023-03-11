﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Data.Patterns.Parameters;
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
	public override T Get<T>(string key)
	{
		T entity = base.Get<T>(key);
		if (entity is IBuildingLookup buildingLookup) buildingLookup.ImageUrl = Repository.GetMainImage(key);
		return entity;
	}

	/// <inheritdoc />
	public override T Get<T>(string key, IGetSettings settings)
	{
		T entity = base.Get<T>(key, settings);
		if (entity is IBuildingLookup buildingLookup) buildingLookup.ImageUrl = Repository.GetMainImage(key);
		return entity;
	}

	/// <inheritdoc />
	public override async Task<T> GetAsync<T>(string key, CancellationToken token = default(CancellationToken))
	{
		T entity = await base.GetAsync<T>(key, token);
		if (entity is IBuildingLookup buildingLookup) buildingLookup.ImageUrl = await Repository.GetMainImageAsync(key, token);
		return entity;
	}

	/// <inheritdoc />
	public override async Task<T> GetAsync<T>(string key, IGetSettings settings, CancellationToken token = new CancellationToken())
	{
		T entity = await base.GetAsync<T>(key, settings, token);
		if (entity is IBuildingLookup buildingLookup) buildingLookup.ImageUrl = await Repository.GetMainImageAsync(key, token);
		return entity;
	}

	/// <inheritdoc />
	public IList<BuildingImage> ListImages(string buildingId)
	{
		ThrowIfDisposed();
		IList<BuildingImage> result = Repository.ListImages(buildingId)
												.ToList();
		return result;
	}

	/// <inheritdoc />
	public Task<IList<BuildingImage>> ListImagesAsync(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.ListImages(buildingId)
						.ToListAsync(token)
						.As<List<BuildingImage>, IList<BuildingImage>>(token);
	}

	/// <inheritdoc />
	public IList<T> ListImages<T>(string buildingId)
	{
		ThrowIfDisposed();
		IList<T> result = Repository.ListImages(buildingId)
														.ProjectTo<T>(Mapper.ConfigurationProvider)
														.ToList();
		return result;
	}

	/// <inheritdoc />
	public Task<IList<T>> ListImagesAsync<T>(string buildingId, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.ListImages(buildingId)
						.ProjectTo<T>(Mapper.ConfigurationProvider)
						.ToListAsync(token)
						.As<List<T>, IList<T>>(token);
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

	/// <inheritdoc />
	public IList<BuildingImage> DeleteImages(int[] id)
	{
		ThrowIfDisposed();
		IList<BuildingImage> images = Repository.DeleteImages(id);
		if (images.Count > 0) Context.SaveChanges();
		return images;
	}

	/// <inheritdoc />
	public async Task<IList<BuildingImage>> DeleteImagesAsync(int[] id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IList<BuildingImage> images = await Repository.DeleteImagesAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (images.Count > 0) await Context.SaveChangesAsync(token);
		return images;
	}

	private string GetImagePath(string fileName)
	{
		return string.IsNullOrEmpty(fileName)
					? null
					: Path.Combine(_assetImagesPath, fileName);
	}
}