using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingImageForList
{
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
	[Display(Name = "الأولوية")]
	public byte? Priority { get; set; }
}