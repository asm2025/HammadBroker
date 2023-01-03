using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class CitiesList : SortablePagination
{
	[Display(Name = "الإسم")]
	public string Search { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
}