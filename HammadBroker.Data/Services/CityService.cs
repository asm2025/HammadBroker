using AutoMapper;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class CityService : Service<DataContext, ICityRepository, City, int>, ICityService
{
	public CityService([NotNull] ICityRepository cityRepository, [NotNull] IMapper mapper, [NotNull] ILogger<CityService> logger)
		: base(cityRepository, mapper, logger)
	{
	}
}