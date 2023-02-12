using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

public class BuildingAdService : Service<DataContext, IBuildingAdRepository, BuildingAd, int>, IBuildingAdService
{
	public BuildingAdService([NotNull] IBuildingAdRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<BuildingAdService> logger)
		: base(repository, mapper, logger)
	{
	}

	/// <inheritdoc />
	public async Task<BuildingAdsPaginated> ListWithBuildingsAsync(BuildingAdList settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForList> queryable = Repository.ListWithBuildings(settings);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
			queryable = queryable.Paginate(settings);
		}

		IList<BuildingAdForList> result = await queryable.ToListAsync(token)
														.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new BuildingAdsPaginated(result, settings);
	}

	/// <inheritdoc />
	public async Task<BuildingAdsForDisplayPaginated> ListActiveWithBuildingsAsync(BuildingAdList settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForDisplay> queryable = Repository.ListActiveWithBuildings(settings);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
			queryable = queryable.Paginate(settings);
		}

		IList<BuildingAdForDisplay> result = await queryable.ToListAsync(token)
															.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new BuildingAdsForDisplayPaginated(result, settings);
	}

	/// <inheritdoc />
	public Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.GetBuildingAsync(id, token);
	}
}