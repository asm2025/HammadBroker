using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;

namespace HammadBroker.Model.DTO;

public class BuildingForDisplay : BuildingForList, IBuildingForDisplay, IAddressLookup
{
	/// <inheritdoc />
	[Display(Name = "الدور")]
	public Floors? Floor { get; set; }
	public string FloorName => Floor?.GetDisplayName();
	/// <inheritdoc />
	[Display(Name = "غرفة")]
	public byte? Rooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "حمام")]
	public byte? Bathrooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "مباني")]
	public long? BuildingArea { get; set; }
	/// <inheritdoc />
	[Display(Name = "أرض")]
	public long? Area { get; set; }
	/// <inheritdoc />
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	/// <inheritdoc />
	[Display(Name = "الحي")]
	public string DistrictName { get; set; }
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public string CityName { get; set; }
	[Display(Name = "تليفون")]
	public string Phone { get; set; }
	[Display(Name = "محمول")]
	public string Mobile { get; set; }
	/// <inheritdoc />
	[Display(Name = "الوصف المختصر")]
	public string ShortDescription { get; set; }
}