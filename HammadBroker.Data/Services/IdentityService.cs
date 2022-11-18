using AutoMapper;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class IdentityService : Service<DataContext, IIdentityRepository, ApplicationUser, string>, IIdentityService
{
    public IdentityService([NotNull] IIdentityRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<IdentityService> logger)
        : base(repository, mapper, logger)
    {
    }

    /// <inheritdoc />
    public UserManager UserManager => Repository.UserManager;

    /// <inheritdoc />
    public RoleManager RoleManager => Repository.RoleManager;
}