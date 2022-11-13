using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Patterns.Object;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public abstract class ServiceBase<TContext> : Disposable, IServiceBase<TContext>
	where TContext : DbContext
{
	protected ServiceBase([NotNull] TContext context, [NotNull] IMapper mapper, [NotNull] ILogger logger)
	{
		Context = context;
		Mapper = mapper;
		Logger = logger;
	}

	/// <inheritdoc />
	public TContext Context { get; }

	/// <inheritdoc />
	public IMapper Mapper { get; }

	/// <inheritdoc />
	public ILogger Logger { get; }
}

public abstract class ServiceBase<TContext, TRepository, TEntity, TKey> : Disposable, IServiceBase<TContext, TRepository, TEntity, TKey>
	where TRepository : IRepository<TContext, TEntity, TKey>
	where TContext : DbContext
	where TEntity : class, IEntity
{
	protected ServiceBase([NotNull] TRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger logger)
	{
		Repository = repository;
		Mapper = mapper;
		Logger = logger;
	}

	/// <inheritdoc />
	public Type EntityType { get; } = typeof(ApplicationUser);

	/// <inheritdoc />
	public TRepository Repository { get; }

	/// <inheritdoc />
	public TContext Context => Repository.Context;

	/// <inheritdoc />
	public IMapper Mapper { get; }

	/// <inheritdoc />
	public ILogger Logger { get; }

	/// <inheritdoc />
	[NotNull]
	public virtual IPaginated<T> List<T>(IPagination settings = null)
	{
		ThrowIfDisposed();

		IQueryable<TEntity> entities = Repository.List(settings);

		if (settings is { PageSize: > 0 })
		{
			entities = entities.Paginate(settings);
			settings.Count = Repository.Count(settings);
		}

		IList<T> result = entities.ProjectTo<T>(Mapper.ConfigurationProvider)
								.ToList();
		return new Paginated<T>(result, settings);
	}

	/// <inheritdoc />
	[ItemNotNull]
	public virtual async Task<IPaginated<T>> ListAsync<T>(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<TEntity> entities = Repository.List(settings);

		if (settings is { PageSize: > 0 })
		{
			entities = entities.Paginate(settings);
			settings.Count = await Repository.CountAsync(settings, token);
			token.ThrowIfCancellationRequested();
		}

		IList<T> result = await entities.ProjectTo<T>(Mapper.ConfigurationProvider)
								.ToListAsync(token)
								.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new Paginated<T>(result, settings);
	}

	/// <inheritdoc />
	public virtual T Get<T>(TKey key)
	{
		ThrowIfDisposed();
		TEntity entity = Repository.Get(key);
		return entity == null
				? default(T)
				: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public virtual async Task<T> GetAsync<T>(TKey key, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		TEntity entity = await Repository.GetAsync(key, token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public virtual T Get<T>(TKey key, IGetSettings settings)
	{
		ThrowIfDisposed();
		TEntity entity = Repository.Get(key, settings);
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}

	/// <inheritdoc />
	public virtual async Task<T> GetAsync<T>(TKey key, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		TEntity entity = await Repository.GetAsync(key, settings, token)
										.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return entity == null
					? default(T)
					: Mapper.Map<T>(entity);
	}
}