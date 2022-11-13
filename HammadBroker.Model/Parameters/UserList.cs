using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

[Serializable]
public class UserList : SortablePagination
{
	public string Name { get; set; }
	public string UserName { get; set; }
	public Genders? Gender { get; set; }
	public int? CityId { get; set; }
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }
}