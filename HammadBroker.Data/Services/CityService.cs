using System.Linq;
using AutoMapper;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
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
	protected override IQueryable<City> PrepareListQuery(IQueryable<City> queryable, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareListQuery(queryable, settings);
		queryable = PrepareQuery(queryable, citiesList);
		return base.PrepareListQuery(queryable, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<City> PrepareCountQuery(IQueryable<City> queryable, IPagination settings)
	{
		if (settings is not CitiesList citiesList) return base.PrepareCountQuery(queryable, settings);
		queryable = PrepareQuery(queryable, citiesList);
		return base.PrepareCountQuery(queryable, settings);
	}

	[NotNull]
	private static IQueryable<City> PrepareQuery([NotNull] IQueryable<City> queryable, [NotNull] CitiesList citiesList)
	{
		if (!string.IsNullOrEmpty(citiesList.CountryCode)) queryable = queryable.Where(e => e.CountryCode == citiesList.CountryCode);
		if (!string.IsNullOrEmpty(citiesList.Search)) queryable = queryable.Where(e => e.Name.Contains(citiesList.Search));
		return queryable;
	}
}