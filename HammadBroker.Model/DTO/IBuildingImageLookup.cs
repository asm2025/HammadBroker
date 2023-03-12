using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.DTO;

public interface IBuildingImageLookup : IEntity<int>
{
	[Display(Name = "الصورة")]
	string ImageUrl { get; set; }
}