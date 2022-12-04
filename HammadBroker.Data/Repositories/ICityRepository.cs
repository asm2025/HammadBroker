using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Repositories;

public interface ICityRepository : IRepository<DataContext, City, int>
{
	[NotNull]
	IQueryable<City> List([NotNull] string countryCode, IPagination settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IList<City>> ListAsync([NotNull] string countryCode, IPagination settings = null, CancellationToken token = default(CancellationToken));
}