﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class CitiesPaginated : Paginated<CityForList>
{
	public CitiesPaginated([NotNull] IEnumerable<CityForList> result, [NotNull] IPagination pagination)
		: base(result, pagination)
	{
	}

	[Display(Name = "البحث")]
	public string Search { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	public IList<CountryForList> Countries { get; set; }
}