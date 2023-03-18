using AutoMapper;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class DistrictService : Service<DataContext, IDistrictRepository, District, int>, IDistrictService
{
	public DistrictService([NotNull] IDistrictRepository districtRepository, [NotNull] IMapper mapper, [NotNull] ILogger<DistrictService> logger)
		: base(districtRepository, mapper, logger)
	{
	}
}