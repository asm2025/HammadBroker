using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class CitiesPaginated : IPaginated<CityForList>
{
	private readonly IPaginated<CityForList> _paginated;

	public CitiesPaginated(IEnumerable<CityForList> result, IPagination pagination)
		: this(new Paginated<CityForList>(result, pagination))
	{
	}

	public CitiesPaginated([NotNull] IPaginated<CityForList> paginated)
	{
		_paginated = paginated;
		if (paginated.Pagination is not CitiesList citiesList) return;
		Search = citiesList.Search;
		CountryCode = citiesList.CountryCode;
	}

	[Display(Name = "البحث")]
	public string Search { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	public IList<CountryForList> Countries { get; set; }

	/// <inheritdoc />
	public IEnumerable<CityForList> Result => _paginated.Result;

	/// <inheritdoc />
	public IPagination Pagination => _paginated.Pagination;
}