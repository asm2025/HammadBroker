using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Service;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Data.Services;

public interface IService<TContext, TRepository, TEntity, TKey> : IServiceBase<TContext, TRepository, TEntity, TKey>, IService<TEntity, TKey>
    where TRepository : IRepository<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity
{
}