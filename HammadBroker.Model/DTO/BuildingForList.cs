using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;

namespace HammadBroker.Model.DTO;

public class BuildingForList : IBuildingLookup, IBuildingImageLookup
{
	/// <inheritdoc />
	[Display(Name = "مسلسل")]
	public int Id { get; set; }
	/// <inheritdoc />
	[Display(Name = "مرجع")]
	public string Reference { get; set; }
	/// <inheritdoc />
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
	/// <inheritdoc />
	[Display(Name = "نوع العقار")]
	public BuildingType BuildingType { get; set; }
	public string BuildingTypeName => BuildingType.GetDisplayName();
	/// <inheritdoc />
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	public string FinishingTypeName => FinishingType?.GetDisplayName();
	/// <inheritdoc />
	[Display(Name = "الحي")]
	public int? DistrictId { get; set; }
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType AdType { get; set; }
	public string AdTypeName => AdType.GetDisplayName();
	[Display(Name = "الاولوية")]
	public byte? Priority { get; set; }
	[Display(Name = "نشر")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime CreatedOn { get; set; }
	[Display(Name = "آخر تحديث")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime UpdatedOn { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime? Expires { get; set; }
	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}", ApplyFormatInEditMode = true)]
	public long Price { get; set; }
	[Display(Name = "مفعل")]
	public bool Enabled { get; set; }
	[Display(Name = "المشاهدات")]
	public long Views { get; set; }
	[Display(Name = "مشاهدات الصفحة")]
	public long PageViews { get; set; }
}