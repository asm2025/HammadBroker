using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace HammadBroker.Extensions;

public static class IQueryableExtension
{
	[NotNull]
	public static IQueryable<T> WhereIf<T>([NotNull] this IQueryable<T> query, bool condition, [NotNull] Expression<Func<T, bool>> predicate)
	{
		return condition
					? query.Where(predicate)
					: query;
	}
}