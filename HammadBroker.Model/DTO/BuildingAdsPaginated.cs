using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdsPaginated : Paginated<BuildingAdForList, BuildingAdList>
{
	public BuildingAdsPaginated([NotNull] IEnumerable<BuildingAdForList> result, [NotNull] BuildingAdList pagination)
		: base(result, pagination)
	{
	}
}