using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForDetails : BuildingForDisplay, IBuilding, IAddressLookup
{
	/// <inheritdoc />
	[Display(Name = "معرف Youtube")]
	public string VideoId { get; set; }
	/// <inheritdoc />
	[Display(Name = "الوصف")]
	public string Description { get; set; }
}