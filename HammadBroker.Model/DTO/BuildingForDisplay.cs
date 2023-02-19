using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForDisplay : BuildingForList, IBuildingForDisplay, ICityNameLookup
{
	/// <inheritdoc />
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }
	/// <inheritdoc />
	[Display(Name = "غرفة")]
	public byte? Rooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "حمام")]
	public byte? Bathrooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "م")]
	public long? Area { get; set; }
	/// <inheritdoc />
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	/// <inheritdoc />
	[Display(Name = "الحي")]
	public string Address2 { get; set; }
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public string CityName { get; set; }
	[Display(Name = "تليفون")]
	public string Phone { get; set; }
	[Display(Name = "محمول")]
	public string Mobile { get; set; }
	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	public long Price { get; set; }
	/// <inheritdoc />
	[Display(Name = "الوصف")]
	public string ShortDescription { get; set; }
}