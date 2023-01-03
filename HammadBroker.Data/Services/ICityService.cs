using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ICityService : IService<DataContext, ICityRepository, City, int>
{
	IPaginated<City> List(string countryCode, IPagination settings = null);
	IPaginated<T> List<T>(string countryCode, IPagination settings = null);
	[NotNull]
	Task<IPaginated<City>> ListAsync(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IPaginated<T>> ListAsync<T>(string countryCode, string search = null, IPagination settings = null, CancellationToken token = default(CancellationToken));
}