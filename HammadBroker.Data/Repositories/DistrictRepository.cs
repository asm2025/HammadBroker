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

public class DistrictRepository : Repository<DataContext, District, int>, IDistrictRepository
{
	/// <inheritdoc />
	public DistrictRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<DistrictRepository> logger)
		: base(context, configuration, logger)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<District> PrepareCountQuery(IQueryable<District> query, IPagination settings)
	{
		if (settings is DistrictList { CityId: > 0 } districtList) query = query.Where(e => e.CityId == districtList.CityId);
		if (settings is SearchList searchList && !string.IsNullOrEmpty(searchList.Search)) query = query.Where(e => e.Name.Contains(searchList.Search));
		return base.PrepareCountQuery(query, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<District> PrepareListQuery(IQueryable<District> query, IPagination settings)
	{
		if (settings is DistrictList { CityId: > 0 } districtList) query = query.Where(e => e.CityId == districtList.CityId);
		if (settings is SearchList searchList && !string.IsNullOrEmpty(searchList.Search)) query = query.Where(e => e.Name.Contains(searchList.Search));
		return base.PrepareListQuery(query, settings);
	}
}