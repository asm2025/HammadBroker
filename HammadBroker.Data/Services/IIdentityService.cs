using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface IIdentityService : IService<DataContext, IIdentityRepository, User, string>
{
    [NotNull]
    UserManager UserManager { get; }
    [NotNull]
    RoleManager RoleManager { get; }
}