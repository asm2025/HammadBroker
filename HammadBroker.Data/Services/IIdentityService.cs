using System.Threading;
using System.Threading.Tasks;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface IIdentityService : IService<DataContext, IIdentityRepository, ApplicationUser, string>
{
	[NotNull]
	UserManager UserManager { get; }
	[NotNull]
	RoleManager RoleManager { get; }

	[NotNull]
	public Task<UserForDetails> GetDetailsAsync([NotNull] string key, CancellationToken token = default(CancellationToken));
}