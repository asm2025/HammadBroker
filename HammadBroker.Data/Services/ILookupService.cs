using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ILookupService : IServiceBase<DataContext>
{
	[NotNull]
	Task<IList<string>> ListBuildingImagesAsync(int id, int count, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IList<CityForList>> ListCitiesAsync([NotNull] SearchList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IList<DistrictForList>> ListDistrictsAsync([NotNull] DistrictList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	[ItemNotNull]
	IList<string> ListBuildingTypes();
	[NotNull]
	[ItemNotNull]
	IList<string> ListFinishingTypes();
	[NotNull]
	Task FillAddressLookupAsync([NotNull] IAddressLookup lookup, CancellationToken token = default(CancellationToken));
}