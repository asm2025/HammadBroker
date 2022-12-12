using System.ComponentModel.DataAnnotations;
using essentialMix.Core.Web.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public class BuildingImageToAdd
{

	[Display(Name = "الصورة")]
	[MaxFileSize(0xA00000)]
	[DataType(DataType.Upload)]
	public IFormFile ImageFile { get; set; }
}