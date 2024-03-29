﻿using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class CitiesPaginated : Paginated<CityForList, SearchList>
{
	public CitiesPaginated([NotNull] IEnumerable<CityForList> result, [NotNull] SearchList pagination)
		: base(result, pagination)
	{
	}
}