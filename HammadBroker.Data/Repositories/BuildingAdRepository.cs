using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class BuildingAdRepository : Repository<DataContext, BuildingAd, int>, IBuildingAdRepository
{
	/// <inheritdoc />
	public BuildingAdRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingAdRepository> logger)
		: base(context, configuration, logger)
	{
	}

	/// <inheritdoc />
	public IQueryable<BuildingAd> ListActive(IPagination settings = null)
	{
		ThrowIfDisposed();
		IQueryable<BuildingAd> query = DbSet.Where(e => e.Expires == null || e.Expires >= DateTime.Today);
		return PrepareListQuery(query, settings);
	}

	/// <inheritdoc />
	public Task<IList<BuildingAd>> ListActiveAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return ListActive(settings)
				.Paginate(settings)
				.ToListAsync(token)
				.As<List<BuildingAd>, IList<BuildingAd>>(token);
	}
}