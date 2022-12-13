using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.DTO;

namespace HammadBroker.Model.Parameters;

public class CitiesList : SortablePagination
{
	[Display(Name = "البحث")]
	public string Search { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	public IList<CountryForList> Countries { get; set; }
}