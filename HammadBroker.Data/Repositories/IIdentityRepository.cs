using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Repositories;

public interface IIdentityRepository : IRepository<DataContext, ApplicationUser, string>
{
	[NotNull]
	UserManager UserManager { get; }
	[NotNull]
	RoleManager RoleManager { get; }
}