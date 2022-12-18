using System;
using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

[Serializable]
public class Paginated<T, TPagination> : Paginated<T>, IPaginated<T, TPagination>
	where TPagination : IPagination
{
	public Paginated([NotNull] IEnumerable<T> result, [NotNull] TPagination pagination)
		: base(result, pagination)
	{
	}

	public TPagination PaginationModel => (TPagination)Pagination;
}