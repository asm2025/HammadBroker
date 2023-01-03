using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Extensions;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
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
	public IQueryable<City> List(string countryCode, string search = null, IPagination settings = null)
	{
		if (string.IsNullOrEmpty(countryCode) && string.IsNullOrEmpty(search)) return List(settings);
		ThrowIfDisposed();
		IQueryable<City> queryable = DbSet
			.Where(e => e.CountryCode == countryCode)
			.WhereIf(!string.IsNullOrEmpty(search), e => e.Name.Contains(search));
		return PrepareListQuery(queryable, settings);
	}

	/// <inheritdoc />
	public Task<IList<City>> ListAsync(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(countryCode) && string.IsNullOrEmpty(search)) return ListAsync(settings, token);

		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<City> queryable = DbSet
			.Where(e => e.CountryCode == countryCode)
			.WhereIf(!string.IsNullOrEmpty(search), e => e.Name.Contains(search));
		return PrepareListQuery(queryable, settings)
				.ToListAsync(token)
				.ConfigureAwait()
				.As<List<City>, IList<City>>(token);
	}

	/// <inheritdoc />
	protected override IQueryable<City> PrepareListQuery(IQueryable<City> query, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareListQuery(query, settings);
		if (!string.IsNullOrEmpty(citiesList.CountryCode)) query = query.Where(e => e.CountryCode == citiesList.CountryCode);
		if (!string.IsNullOrEmpty(citiesList.Search)) query = query.Where(e => e.Name.Contains(citiesList.Search));
		return base.PrepareListQuery(query, settings);
	}
}