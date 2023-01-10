using System.ComponentModel.DataAnnotations;
using essentialMix.Core.Web.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate
{
	[Display(Name = "اسم العقار")]
	[Required]
	[StringLength(256)]
	public string Name { get; set; }

	[Display(Name = "الصورة الرئيسية")]
	[MaxFileSize(Constants.Images.FileSizeMax)]
	[DataType(DataType.Upload)]
	public IFormFile Image { get; set; }

	[Display(Name = "الصورة الرئيسية")]
	[StringLength(320)]
	[DataType(DataType.ImageUrl)]
	public string ImageUrl { get; set; }

	[Display(Name = "الفيديو")]
	[StringLength(128)]
	public string VideoId { get; set; }

	[Display(Name = "نوع العقار")]
	[Required]
	public BuildingType BuildingType { get; set; }

	[Display(Name = "نوع التشطيب")]
	[Required]
	public FinishingType? FinishingType { get; set; }

	[Display(Name = "الدور")]
	public byte? Floor { get; set; }

	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }

	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }

	[Display(Name = "المساحة")]
	[Range(1, 1000000000)]
	public long? Area { get; set; }

	[Display(Name = "العنوان")]
	[StringLength(256)]
	public string Address { get; set; }

	[Display(Name = "العنوان 2")]
	[StringLength(256)]
	public string Address2 { get; set; }

	[Display(Name = "البلد")]
	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }

	[Display(Name = "المدينة")]
	[Required]
	public int CityId { get; set; }

	[Display(Name = "الوصف")]
	[StringLength(2048)]
	[DataType(DataType.MultilineText)]
	public string Description { get; set; }
}