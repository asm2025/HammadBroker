using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Core.Web.Annotations;
using Microsoft.AspNetCore.Http;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate
{
	[Required]
	[StringLength(256)]
	[Display(Name = "اسم المبنى")]
	public string Name { get; set; }

	[MaxFileSize(0xA00000)]
	[DataType(DataType.Upload)]
	[Display(Name = "الصورة الرئيسية")]
	public IFormFile ImageFile { get; set; }

	[Url]
	[StringLength(320)]
	[Display(Name = "الصورة الرئيسية")]
	public string ImageUrl { get; set; }

	[Url]
	[StringLength(320)]
	[Display(Name = "الفيديو")]
	public string VideoUrl { get; set; }

	[Required]
	[Display(Name = "نوع العقار")]
	public BuildingType BuildingType { get; set; }

	[Required]
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }

	[Display(Name = "الدور")]
	public byte? Floor { get; set; }

	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }

	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }

	[Display(Name = "المساحة")]
	public decimal? Area { get; set; }

	[StringLength(2048)]
	[Display(Name = "الموقع")]
	public string Location { get; set; }

	[StringLength(512)]
	[Display(Name = "العنوان")]
	public string Address { get; set; }

	[StringLength(128)]
	[Display(Name = "العنوان 2")]
	public string Address2 { get; set; }

	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	public IList<CityForList> Cities { get; set; }

	[Required]
	[StringLength(2048)]
	[Display(Name = "الوصف")]
	public string Description { get; set; }

	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	public IList<CountryForList> Countries { get; set; }
}