using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingForDisplay : IBuildingLookup
{
	[Display(Name = "العنوان")]
	string Address { get; set; }
	[Display(Name = "الدور")]
	Floors? Floor { get; set; }
	string FloorName { get; }
	[Display(Name = "الغرف")]
	byte? Rooms { get; set; }
	[Display(Name = "الحمامات")]
	byte? Bathrooms { get; set; }
	[Display(Name = "مساحة المبانى")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	long? BuildingArea { get; set; }
	[Display(Name = "مساحة الأرض")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	long? Area { get; set; }
	[Display(Name = "تليفون")]
	public string Phone { get; set; }
	[Display(Name = "محمول")]
	public string Mobile { get; set; }
	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	public long Price { get; set; }
	[Display(Name = "الوصف المختصر")]
	public string ShortDescription { get; set; }
}