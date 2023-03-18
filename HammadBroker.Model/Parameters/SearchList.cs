using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class SearchList : SortablePagination
{
	[Display(Name = "البحث")]
	public string Search { get; set; }

	public virtual bool HasSearch => !string.IsNullOrEmpty(Search);
}