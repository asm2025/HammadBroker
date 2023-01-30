using System.Linq;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
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
	protected override IQueryable<City> PrepareCountQuery(IQueryable<City> query, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareCountQuery(query, settings);
		if (!string.IsNullOrEmpty(citiesList.CountryCode)) query = query.Where(e => e.CountryCode == citiesList.CountryCode);
		if (!string.IsNullOrEmpty(citiesList.Search)) query = query.Where(e => e.Name.Contains(citiesList.Search));
		return base.PrepareCountQuery(query, settings);
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