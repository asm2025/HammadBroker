using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;
using HammadBroker.Model.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate : IBuilding
{
	private const string PHONE_OR_MOBILE_REQUIRED = "Any of the fields '{0}' is required.";

	/// <inheritdoc />
	[Display(Name = "مرجع")]
	[StringLength(Constants.Buildings.IdentifierLength)]
	public string Reference { get; set; }

	/// <inheritdoc />
	[Display(Name = "معرف Youtube")]
	[StringLength(128)]
	public string VideoId { get; set; }

	/// <inheritdoc />
	[Display(Name = "نوع العقار")]
	[Required]
	public BuildingType BuildingType { get; set; }

	public string BuildingTypeName => BuildingType.GetDisplayName();

	/// <inheritdoc />
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }

	public string FinishingTypeName => FinishingType?.GetDisplayName();

	/// <inheritdoc />
	[Display(Name = "الدور")]
	public Floors? Floor { get; set; }

	public string FloorName => Floor?.GetDisplayName();

	/// <inheritdoc />
	[Display(Name = "الغرف")]
	[Range(0, byte.MaxValue)]
	public byte? Rooms { get; set; }

	/// <inheritdoc />
	[Display(Name = "الحمامات")]
	[Range(0, byte.MaxValue)]
	public byte? Bathrooms { get; set; }

	/// <inheritdoc />
	[Display(Name = "مساحة المبني")]
	[Range(0, long.MaxValue)]
	public long? BuildingArea { get; set; }

	/// <inheritdoc />
	[Display(Name = "المساحة")]
	[Range(0, long.MaxValue)]
	public long? Area { get; set; }

	/// <inheritdoc />
	[Display(Name = "العنوان")]
	[StringLength(256)]
	public string Address { get; set; }

	/// <inheritdoc />
	[Display(Name = "الحي")]
	[Range(0, int.MaxValue)]
	public int? DistrictId { get; set; }

	[StringLength(256)]
	public string DistrictName { get; set; }

	/// <inheritdoc />
	[Display(Name = "المدينة")]
	[Required]
	[Range(1, int.MaxValue)]
	public int CityId { get; set; }

	/// <inheritdoc />
	[Display(Name = "الوصف المختصر")]
	[StringLength(1024)]
	public string ShortDescription { get; set; }

	/// <inheritdoc />
	[Display(Name = "الوصف")]
	[Required]
	[StringLength(4096)]
	[DataType(DataType.MultilineText)]
	public string Description { get; set; }

	[Display(Name = "النوع")]
	[Required]
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
	[Required]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime Date { get; set; }

	[Display(Name = "تاريخ الانتهاء")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime? Expires { get; set; }

	[Display(Name = "التليفون")]
	[Phone]
	[RequireAnyOf(nameof(Phone), nameof(Mobile), ErrorMessage = PHONE_OR_MOBILE_REQUIRED)]
	[DataType(DataType.PhoneNumber)]
	public string Phone { get; set; }

	[Display(Name = "المحمول")]
	[Phone]
	[RequireAnyOf(nameof(Phone), nameof(Mobile), ErrorMessage = PHONE_OR_MOBILE_REQUIRED)]
	[DataType(DataType.PhoneNumber)]
	public string Mobile { get; set; }

	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	[Required]
	[Range(1, long.MaxValue)]
	public long Price { get; set; }

	[Display(Name = "مفعل")]
	public bool Enabled { get; set; } = true;
	[Display(Name = "المشاهدات")]
	public long Views { get; set; }
	[Display(Name = "مشاهدات الصفحة")]
	public long PageViews { get; set; }
}