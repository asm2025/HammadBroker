using AutoMapper;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class AdService : Service<DataContext, IAdRepository, Ad, long>, IAdService
{
	public AdService([NotNull] IAdRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<AdService> logger)
		: base(repository, mapper, logger)
	{
	}
}