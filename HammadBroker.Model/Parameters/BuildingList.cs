using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class BuildingList : SortablePagination
{
	[Display(Name = "الرقم")]
	public int? Id { get; set; }
	[Display(Name = "البحث")]
	public string Search { get; set; }
	[Display(Name = "نوع العقار")]
	public BuildingType? BuildingType { get; set; }
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }
	[Display(Name = "بحد أقصى")]
	public byte? MaxFloor { get; set; }
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }
	[Display(Name = "بحد أقصى")]
	public byte? MaxRooms { get; set; }
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }
	[Display(Name = "بحد أقصى")]
	public byte? MaxBathrooms { get; set; }
	[Display(Name = "المساحة")]
	public long? Area { get; set; }
	[Display(Name = "بحد أقصى")]
	public long? MaxArea { get; set; }
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	[Display(Name = "المدينة")]
	public int CityId { get; set; }

	public virtual bool HasSearch => !string.IsNullOrEmpty(Search)
							|| BuildingType.HasValue
							|| FinishingType.HasValue
							|| Floor.HasValue
							|| MaxFloor.HasValue
							|| Rooms.HasValue
							|| MaxRooms.HasValue
							|| Bathrooms.HasValue
							|| MaxBathrooms.HasValue
							|| Area.HasValue
							|| MaxArea.HasValue
							|| !string.IsNullOrEmpty(Address)
							|| !string.IsNullOrEmpty(CountryCode)
							|| CityId > 0;
}