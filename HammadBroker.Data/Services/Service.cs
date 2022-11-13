using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Model;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public abstract class Service<TContext, TRepository, TEntity, TKey> : ServiceBase<TContext, TRepository, TEntity, TKey>, IService<TContext, TRepository, TEntity, TKey>
    where TRepository : IRepository<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity
{
    protected Service([NotNull] TRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger logger)
        : base(repository, mapper, logger)
    {
    }

    /// <inheritdoc />
    public virtual T Add<T>(TEntity entity)
    {
        ThrowIfDisposed();
        entity = Repository.Add(Mapper.Map<TEntity>(entity));
        if (entity == null) return default(T);
        Context.SaveChanges();
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync<T>(TEntity entity, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        entity = await Repository.AddAsync(Mapper.Map<TEntity>(entity), token);
        token.ThrowIfCancellationRequested();
        if (entity == null) return default(T);
        await Context.SaveChangesAsync(token);
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual T Update<T>(TEntity entity)
    {
        ThrowIfDisposed();
        entity = Repository.Update(entity);
        if (entity == null) return default(T);
        Context.SaveChanges();
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync<T>(TEntity entity, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        entity = await Repository.UpdateAsync(entity, token);
        token.ThrowIfCancellationRequested();
        if (entity == null) return default(T);
        await Context.SaveChangesAsync(token);
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual T Delete<T>(TKey key)
    {
        ThrowIfDisposed();
        TEntity entity = Repository.Delete(key);
        if (entity == null) return default(T);
        Context.SaveChanges();
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual async Task<T> DeleteAsync<T>(TKey key, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        TEntity entity = await Repository.DeleteAsync(key, token);
        token.ThrowIfCancellationRequested();
        if (entity == null) return default(T);
        await Context.SaveChangesAsync(token)
                        .ConfigureAwait();
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual T Delete<T>(TEntity entity)
    {
        ThrowIfDisposed();
        entity = Repository.Delete(entity);
        if (entity == null) return default(T);
        Context.SaveChanges();
        return Mapper.Map<T>(entity);
    }

    /// <inheritdoc />
    public virtual async Task<T> DeleteAsync<T>(TEntity entity, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        entity = await Repository.DeleteAsync(entity, token);
        token.ThrowIfCancellationRequested();
        if (entity == null) return default(T);
        await Context.SaveChangesAsync(token)
                        .ConfigureAwait();
        return Mapper.Map<T>(entity);
    }
}