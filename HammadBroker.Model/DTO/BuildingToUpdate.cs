using System.ComponentModel.DataAnnotations;
using essentialMix.Core.Web.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate : IBuilding, IImageUpload
{
	/// <inheritdoc />
	[Display(Name = "اسم العقار")]
	[Required]
	[StringLength(256)]
	public string Name { get; set; }

	/// <inheritdoc />
	[Display(Name = "الصورة")]
	[MaxFileSize(Constants.Images.FileSizeMax)]
	[DataType(DataType.Upload)]
	public IFormFile Image { get; set; }

	/// <inheritdoc />
	[Display(Name = "الصورة")]
	[StringLength(320)]
	[DataType(DataType.ImageUrl)]
	public string ImageUrl { get; set; }

	/// <inheritdoc />
	[Display(Name = "معرف Youtube")]
	[StringLength(128)]
	public string VideoId { get; set; }

	/// <inheritdoc />
	[Display(Name = "نوع العقار")]
	[Required]
	public BuildingType BuildingType { get; set; }

	/// <inheritdoc />
	[Display(Name = "نوع التشطيب")]
	[Required]
	public FinishingType? FinishingType { get; set; }

	/// <inheritdoc />
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }

	/// <inheritdoc />
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }

	/// <inheritdoc />
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }

	/// <inheritdoc />
	[Display(Name = "المساحة")]
	[Range(1, 1000000000)]
	public long? Area { get; set; }

	/// <inheritdoc />
	[Display(Name = "العنوان")]
	[StringLength(256)]
	public string Address { get; set; }

	/// <inheritdoc />
	[Display(Name = "العنوان 2")]
	[StringLength(256)]
	public string Address2 { get; set; }

	/// <inheritdoc />
	[Display(Name = "البلد")]
	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }

	/// <inheritdoc />
	[Display(Name = "المدينة")]
	[Required]
	public int CityId { get; set; }

	/// <inheritdoc />
	[Display(Name = "الوصف")]
	[StringLength(2048)]
	[DataType(DataType.MultilineText)]
	public string Description { get; set; }
}