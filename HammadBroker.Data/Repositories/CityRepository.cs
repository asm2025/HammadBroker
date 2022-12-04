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

public class CityRepository : Repository<DataContext, City, int>, ICityRepository
{
	/// <inheritdoc />
	public CityRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<CityRepository> logger)
		: base(context, configuration, logger)
	{
	}

	/// <inheritdoc />
	public IQueryable<City> List(string countryCode, IPagination settings = null)
	{
		ThrowIfDisposed();
		IQueryable<City> queryable = DbSet.Where(e => e.CountryCode == countryCode);
		return PrepareListQuery(queryable, settings);
	}

	/// <inheritdoc />
	public Task<IList<City>> ListAsync(string countryCode, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		IQueryable<City> queryable = DbSet.Where(e => e.CountryCode == countryCode);
		return PrepareListQuery(queryable, settings).Paginate(settings).ToListAsync(token).As<List<City>, IList<City>>(token);
	}
}