using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace HammadBroker.Model.Entities;

public class Role : IdentityRole<string>
{
    public const string Administrators = "Admin";
    public const string Members = "Member";

    public const int AdministratorsRank = int.MaxValue;

    public static readonly IReadOnlyDictionary<string, int> Roles = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        {Members, 0},
        {Administrators, AdministratorsRank}
    });

    /// <inheritdoc />
    public Role()
    {
    }

    /// <inheritdoc />
    public Role(string roleName)
        : base(roleName)
    {
    }

    public static int GetRank(string role)
    {
        return string.IsNullOrEmpty(role) || !Roles.TryGetValue(role, out int r)
                    ? -1
                    : r;
    }

    public static int GetHighestRank(IEnumerable<string> roles)
    {
        if (roles == null) return -1;
        return roles
                .DefaultIfEmpty()
                .Max(e => string.IsNullOrEmpty(e) || !Roles.TryGetValue(e, out int r)
                            ? -1
                            : r);
    }
}