using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingForDisplay : IBuildingLookup
{
	[Display(Name = "العنوان")]
	string Address { get; set; }
	[Display(Name = "الحي")]
	string Address2 { get; set; }
	[Display(Name = "الدور")]
	byte? Floor { get; set; }
	[Display(Name = "الغرف")]
	byte? Rooms { get; set; }
	[Display(Name = "الحمامات")]
	byte? Bathrooms { get; set; }
	[Display(Name = "المساحة")]
	long? Area { get; set; }
	[Display(Name = "تليفون")]
	public string Phone { get; set; }
	[Display(Name = "محمول")]
	public string Mobile { get; set; }
	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	public long Price { get; set; }
	[Display(Name = "الوصف")]
	public string ShortDescription { get; set; }
}