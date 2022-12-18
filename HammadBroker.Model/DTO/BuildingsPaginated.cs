using System.Collections.Generic;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingsPaginated : Paginated<BuildingForList, BuildingList>
{
	public BuildingsPaginated([NotNull] IEnumerable<BuildingForList> result, [NotNull] BuildingList pagination)
		: base(result, pagination)
	{
	}
}