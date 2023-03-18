using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class DistrictsPaginated : Paginated<DistrictForList, DistrictList>
{
	public DistrictsPaginated([NotNull] IEnumerable<DistrictForList> result, [NotNull] DistrictList pagination)
		: base(result, pagination)
	{
	}
}