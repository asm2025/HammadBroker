using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class CitiesList : SortablePagination
{
	public string Search { get; set; }
	public string Countrycode { get; set; }
}