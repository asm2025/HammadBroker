using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Data.Context;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class LookupService : ServiceBase<DataContext>, ILookupService
{
    public LookupService([NotNull] DataContext context, [NotNull] IMapper mapper, [NotNull] ILogger<LookupService> logger)
        : base(context, mapper, logger)
    {
    }

    /// <inheritdoc />
    public Task<IList<CityForList>> ListCitiesAsync(string countryCode, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        countryCode = countryCode.Trim();
        if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
        return Context.Cities
                      .Where(e => e.CountryCode == countryCode)
                      .ProjectTo<CityForList>(Mapper.ConfigurationProvider)
                      .ToListAsync(token)
                      .As<List<CityForList>, IList<CityForList>>(token);
    }

    /// <inheritdoc />
    public Task<IList<CountryForList>> ListCountriesAsync(CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        return Context.Countries
                      .ProjectTo<CountryForList>(Mapper.ConfigurationProvider)
                      .ToListAsync(token)
                      .As<List<CountryForList>, IList<CountryForList>>(token);
    }

    /// <inheritdoc />
    public IList<string> ListBuildingTypes(CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        return EnumHelper<BuildingType>.GetDisplayNames().ToList();
    }

    /// <inheritdoc />
    public IList<string> ListFinishingTypes(CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        return EnumHelper<FinishingType>.GetDisplayNames().ToList();
    }

    /// <inheritdoc />
    public async Task<IList<string>> ListFloorsAsync(CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();

        List<string> result = await Context.Floors
                                            .Select(e => e.Id)
                                            .ToListAsync(token);
        result.AddRange(Enumerable.Range(1, 30).Select(e => e.ToString()));
        return result;
    }
}