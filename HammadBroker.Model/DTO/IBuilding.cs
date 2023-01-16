using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuilding : IBuildingLookup
{
	[Display(Name = "معرف Youtube")]
	string VideoId { get; set; }
	[Display(Name = "الدور")]
	byte? Floor { get; set; }
	[Display(Name = "الغرف")]
	byte? Rooms { get; set; }
	[Display(Name = "الحمامات")]
	byte? Bathrooms { get; set; }
	[Display(Name = "المساحة")]
	long? Area { get; set; }
	[Display(Name = "العنوان")]
	string Address { get; set; }
	[Display(Name = "الحي")]
	string Address2 { get; set; }
	[Display(Name = "الوصف")]
	string Description { get; set; }
}