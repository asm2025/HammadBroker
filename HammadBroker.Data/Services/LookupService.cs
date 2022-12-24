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
	public Task<IList<CountryForList>> ListCountriesAsync(CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Context.Countries
					.AsNoTracking()
					.OrderBy(e => e.Name)
					.ProjectTo<CountryForList>(Mapper.ConfigurationProvider)
					.ToListAsync(token)
					.As<List<CountryForList>, IList<CountryForList>>(token);
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
	public async Task FillCountriesAsync(ICountryLookup lookup, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		lookup.Countries = await ListCountriesAsync(token).ConfigureAwait();
	}

	/// <inheritdoc />
	public async Task FillCitiesAsync(ICityLookup lookup, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		lookup.Cities = await _cityRepository.List(lookup.CountryCode)
											.ProjectTo<CityForList>(Mapper.ConfigurationProvider)
											.ToListAsync(token)
											.ConfigureAwait();
	}

	/// <inheritdoc />
	public async Task FillCountryNameAsync(ICountryNameLookup lookup, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		lookup.CountryName = null;
		if (string.IsNullOrEmpty(lookup.CountryCode)) return;
		Country country = await Context.Countries
										.FirstOrDefaultAsync(e => e.Id == lookup.CountryCode, token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		if (country == null) return;
		lookup.CountryName = country.Name;
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

	/// <inheritdoc />
	public async Task FillBuildingImagesAsync(int id, IBuildingImagesLookup lookup, int count, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<string> queryable = Context.BuildingImages
												.Where(e => e.BuildingId == id)
												.Select(e => e.ImageUrl);
		if (count > 0) queryable = queryable.Take(count);
		lookup.Images = await queryable.ToListAsync(token)
										.ConfigureAwait();
	}
}