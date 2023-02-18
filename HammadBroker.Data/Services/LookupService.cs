using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class LookupService : ServiceBase<DataContext>, ILookupService
{
	private readonly ICityRepository _cityRepository;

	public LookupService([NotNull] DataContext context, [NotNull] ICityRepository cityRepository, [NotNull] IMapper mapper, [NotNull] ILogger<LookupService> logger)
		: base(context, mapper, logger)
	{
		_cityRepository = cityRepository;
	}

	/// <inheritdoc />
	public Task<IList<string>> ListBuildingImagesAsync(int id, int count, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<string> queryable = Context.BuildingImages
											.Where(e => e.BuildingId == id)
											.Select(e => e.ImageUrl);
		if (count > 0) queryable = queryable.Take(count);
		return queryable.ToListAsync(token)
						.As<List<string>, IList<string>>(token);
	}

	/// <inheritdoc />
	public Task<IList<CityForList>> ListCitiesAsync(CitiesList settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return _cityRepository.List(settings)
							.ProjectTo<CityForList>(Mapper.ConfigurationProvider)
							.ToListAsync(token)
							.As<List<CityForList>, IList<CityForList>>(token);
	}

	/// <inheritdoc />
	public IList<string> ListBuildingTypes()
	{
		ThrowIfDisposed();
		return EnumHelper<BuildingType>
				.GetDisplayNames()
				.ToList();
	}

	/// <inheritdoc />
	public IList<string> ListFinishingTypes()
	{
		ThrowIfDisposed();
		return EnumHelper<FinishingType>
				.GetDisplayNames()
				.ToList();
	}

	/// <inheritdoc />
	public async Task FillCityNameAsync(ICityNameLookup lookup, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		lookup.CityName = null;
		if (lookup.CityId < 1) return;
		City city = await Context.Cities
								.FirstOrDefaultAsync(e => e.Id == lookup.CityId, token)
								.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		if (city == null) return;
		lookup.CityName = city.Name;
	}
}