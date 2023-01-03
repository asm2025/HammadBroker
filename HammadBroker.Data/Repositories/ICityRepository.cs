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
	IQueryable<City> List(string countryCode, string search = null, IPagination settings = null);
	[NotNull]
	Task<IList<City>> ListAsync(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken));
}