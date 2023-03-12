using System.ComponentModel.DataAnnotations;
using essentialMix.Core.Web.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public class BuildingImageToAdd : IImageUpload
{
	[Display(Name = "الصورة")]
	[Required]
	[MaxFileSize(Constants.Images.FileSizeMax)]
	[DataType(DataType.Upload)]
	public IFormFile Image { get; set; }
	[Display(Name = "الأولوية")]
	public byte? Priority { get; set; }
}
