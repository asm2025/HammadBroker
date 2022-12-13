using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingsPaginated : Paginated<BuildingForList>
{
	public BuildingsPaginated([NotNull] IEnumerable<BuildingForList> result, [NotNull] IPagination pagination)
		: base(result, pagination)
	{
	}

	[Display(Name = "البحث")]
	public string Search { get; set; }
}