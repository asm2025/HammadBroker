using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class CityService : Service<DataContext, ICityRepository, City, int>, ICityService
{
	public CityService([NotNull] ICityRepository cityRepository, [NotNull] IMapper mapper, [NotNull] ILogger<CityService> logger)
		: base(cityRepository, mapper, logger)
	{
	}

	/// <inheritdoc />
	public IPaginated<T> List<T>(string countryCode, IPagination settings = null)
	{
		ThrowIfDisposed();
		countryCode = countryCode.Trim();
		if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
		IList<T> result = Repository.List(countryCode, settings)
									.ProjectTo<T>(Mapper.ConfigurationProvider)
									.ToList();
		return new Paginated<T>(result, settings);
	}

	/// <inheritdoc />
	public async Task<IPaginated<T>> ListAsync<T>(string countryCode, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		countryCode = countryCode.Trim();
		if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));

		IQueryable<City> queryable = Repository.List(countryCode, settings);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();
			queryable = queryable.Paginate(settings);
		}

		IList<T> result = await queryable.ProjectTo<T>(Mapper.ConfigurationProvider)
										.ToListAsync(token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new Paginated<T>(result, settings);
	}
}