using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
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
	public async Task<IPaginated<BuildingAdForList>> ListWithBuildingsAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
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
		return new Paginated<BuildingAdForList>(result, settings);
	}

	/// <inheritdoc />
	public Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Repository.GetBuildingAsync(id, token);
	}
}