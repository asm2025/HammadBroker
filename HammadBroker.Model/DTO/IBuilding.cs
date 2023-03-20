using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuilding : IBuildingForDisplay
{
	[Display(Name = "معرف Youtube")]
	string VideoId { get; set; }
	string DistrictName { get; set; }
	[Display(Name = "الوصف")]
	string Description { get; set; }
}