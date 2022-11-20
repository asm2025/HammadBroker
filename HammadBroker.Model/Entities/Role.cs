using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace HammadBroker.Model.Entities;

public class Role : IdentityRole<string>
{
    public const string System = "System";
    public const string Administrators = "Admin";
    public const string Members = "Member";

    public const int AdministratorsRank = int.MaxValue - 1;

    public static readonly IReadOnlyDictionary<string, int> Roles = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        {Members, 0},
        {Administrators, AdministratorsRank},
        {System, AdministratorsRank + 1}
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
        return roles.Max(e => Roles.TryGetValue(e, out int r)
                                ? r
                                : -1);
    }
}