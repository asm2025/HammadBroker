using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class BuildingList : SortablePagination
{
	public string Search { get; set; }
	public BuildingType? BuildingType { get; set; }
	public FinishingType? FinishingType { get; set; }
	public byte? Floor { get; set; }
	public byte? Rooms { get; set; }
	public byte? MaxRooms { get; set; }
	public byte? Bathrooms { get; set; }
	public byte? MaxBathrooms { get; set; }
	public decimal? Area { get; set; }
	public decimal? MaxArea { get; set; }
	public string Address { get; set; }
	public int? CityId { get; set; }
}