using System.Threading;
using System.Threading.Tasks;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ICityService : IService<DataContext, ICityRepository, City, int>
{
	[NotNull]
	IPaginated<T> List<T>([NotNull] string countryCode, IPagination settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IPaginated<T>> ListAsync<T>([NotNull] string countryCode, IPagination settings = null, CancellationToken token = default(CancellationToken));
}