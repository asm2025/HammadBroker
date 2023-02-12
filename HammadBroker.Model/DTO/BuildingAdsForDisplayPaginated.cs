using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdsForDisplayPaginated : Paginated<BuildingAdForDisplay, BuildingAdList>
{
	public BuildingAdsForDisplayPaginated([NotNull] IEnumerable<BuildingAdForDisplay> result, [NotNull] BuildingAdList pagination)
		: base(result, pagination)
	{
	}
}