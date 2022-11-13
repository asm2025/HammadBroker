using AutoMapper;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Service;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public interface IServiceBase<TContext> : IServiceBase
	where TContext : DbContext
{
	[NotNull]
	TContext Context { get; }
	[NotNull]
	IMapper Mapper { get; }
	[NotNull]
	ILogger Logger { get; }
}

public interface IServiceBase<TContext, TRepository, TEntity, TKey> : IServiceBase<TEntity, TKey>
	where TContext : DbContext
	where TRepository : IRepository<TContext, TEntity, TKey>
	where TEntity : class, IEntity
{
	[NotNull]
	TRepository Repository { get; }
	[NotNull]
	TContext Context { get; }
	[NotNull]
	IMapper Mapper { get; }
	[NotNull]
	ILogger Logger { get; }
}