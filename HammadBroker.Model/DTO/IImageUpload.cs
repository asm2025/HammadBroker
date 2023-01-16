using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public interface IImageUpload
{
	[Display(Name = "الصورة")]
	IFormFile Image { get; set; }
}