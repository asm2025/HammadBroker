using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HammadBroker.Data.Context;
using HammadBroker.Data.Identity;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
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

    /// <inheritdoc />
    public async Task<UserForDetails> GetDetailsAsync(string key, CancellationToken token = default(CancellationToken))
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        UserDetailsLookup lookup = await UserManager.Users
                                                    .Join(Context.Cities, u => u.CityId, c => c.Id, (u, c) => new
                                                    {
                                                        u,
                                                        c
                                                    })
                                                    .Join(Context.Countries, uc => uc.u.CountryCode, c => c.Id, (uc, c) => new UserDetailsLookup
                                                    {
                                                        User = uc.u,
                                                        City = uc.c,
                                                        Country = c
                                                    })
                                                    .FirstOrDefaultAsync(e => e.User.Id == key, token);
        token.ThrowIfCancellationRequested();
        if (lookup == null) return null;

        UserForDetails result = Mapper.Map<UserForDetails>(lookup.User);
        Mapper.Map(lookup.City, result);
        Mapper.Map(lookup.Country, result);
        return result;
    }
}