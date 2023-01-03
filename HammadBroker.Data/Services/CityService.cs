using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Extensions;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
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
	[NotNull]
	public IPaginated<City> List(string countryCode, IPagination settings = null)
	{
		ThrowIfDisposed();
		IQueryable<City> queryable = string.IsNullOrEmpty(countryCode)
										? Repository.DbSet
										: Repository.DbSet.Where(e => e.CountryCode == countryCode);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = PrepareCountQuery(queryable, settings).Count();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
		}

		queryable = PrepareListQuery(queryable, settings);
		IList<City> result = queryable.ToList();
		return new Paginated<City>(result, settings);
	}

	/// <inheritdoc />
	[NotNull]
	public IPaginated<T> List<T>(string countryCode, IPagination settings = null)
	{
		ThrowIfDisposed();
		IQueryable<City> queryable = string.IsNullOrEmpty(countryCode)
										? Repository.DbSet
										: Repository.DbSet.Where(e => e.CountryCode == countryCode);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = PrepareCountQuery(queryable, settings).Count();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
		}

		queryable = PrepareListQuery(queryable, settings);
		IList<T> result = queryable.ProjectTo<T>(Mapper.ConfigurationProvider).ToList();
		return new Paginated<T>(result, settings);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public async Task<IPaginated<City>> ListAsync(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<City> queryable = Repository.DbSet
												.WhereIf(!string.IsNullOrEmpty(countryCode), e => e.CountryCode == countryCode)
												.WhereIf(!string.IsNullOrEmpty(search), e => e.Name.Contains(search));

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await PrepareCountQuery(queryable, settings).CountAsync(token);
			token.ThrowIfCancellationRequested();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
		}

		queryable = PrepareListQuery(queryable, settings);
		IList<City> result = await queryable.ToListAsync(token);
		token.ThrowIfCancellationRequested();
		return new Paginated<City>(result, settings);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public async Task<IPaginated<T>> ListAsync<T>(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		IQueryable<City> queryable = Repository.DbSet
												.WhereIf(!string.IsNullOrEmpty(countryCode), e => e.CountryCode == countryCode)
												.WhereIf(!string.IsNullOrEmpty(search), e => e.Name.Contains(search));

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await PrepareCountQuery(queryable, settings).CountAsync(token);
			token.ThrowIfCancellationRequested();
			int maxPages = (int)Math.Ceiling(settings.Count / (double)settings.PageSize);
			if (settings.Page > maxPages) settings.Page = maxPages;
		}

		queryable = PrepareListQuery(queryable, settings);
		IList<T> result = await queryable.ProjectTo<T>(Mapper.ConfigurationProvider).ToListAsync(token);
		token.ThrowIfCancellationRequested();
		return new Paginated<T>(result, settings);
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