using System.Linq;
using AutoMapper;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class CityService : Service<DataContext, ICityRepository, City, int>, ICityService
{
	public CityService([NotNull] ICityRepository cityRepository, [NotNull] IMapper mapper, [NotNull] ILogger<CityService> logger)
		: base(cityRepository, mapper, logger)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<City> PrepareList(IQueryable<City> queryable, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareList(queryable, settings);
		queryable = PrepareQuery(queryable, citiesList);
		return base.PrepareList(queryable, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<City> PrepareCount(IQueryable<City> queryable, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareCount(queryable, settings);
		queryable = PrepareQuery(queryable, citiesList);
		return base.PrepareCount(queryable, settings);
	}

	[NotNull]
	private static IQueryable<City> PrepareQuery([NotNull] IQueryable<City> queryable, [NotNull] CitiesList citiesList)
	{
		if (!string.IsNullOrEmpty(citiesList.CountryCode)) queryable = queryable.Where(e => e.CountryCode == citiesList.CountryCode);
		if (!string.IsNullOrEmpty(citiesList.Search)) queryable = queryable.Where(e => e.Name.Contains(citiesList.Search));
		return queryable;
	}
}