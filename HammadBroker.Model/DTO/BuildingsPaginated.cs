using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingsPaginated<TBuilding> : Paginated<TBuilding, BuildingList>
{
	public BuildingsPaginated([NotNull] IEnumerable<TBuilding> result, [NotNull] BuildingList pagination)
		: base(result, pagination)
	{
	}
}