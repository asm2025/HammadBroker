using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public interface IPaginated<out T, out TPagination> : IPaginated<T>
	where TPagination : IPagination
{
	[NotNull]
	TPagination PaginationModel { get; }
}