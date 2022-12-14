using System;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.DTO;

[Serializable]
public class UserList : SortablePagination
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public Genders? Gender { get; set; }
}