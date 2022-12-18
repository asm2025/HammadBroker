using System.Collections.Generic;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class CitiesPaginated : Paginated<CityForList, CitiesList>
{
	public CitiesPaginated([NotNull] IEnumerable<CityForList> result, [NotNull] CitiesList pagination)
		: base(result, pagination)
	{
	}

	public IList<CountryForList> Countries { get; set; }
}