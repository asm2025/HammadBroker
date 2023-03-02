using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate : IBuilding
{
	/// <inheritdoc />
	string IBuildingLookup.Id { get; set; }

	/// <inheritdoc />
	string IBuildingLookup.ImageUrl { get; set; }

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
	[Required]
	public FinishingType? FinishingType { get; set; }

	public string FinishingTypeName => FinishingType?.GetDisplayName();

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
	[Display(Name = "المدينة")]
	[Required]
	public int CityId { get; set; }

	/// <inheritdoc />
	[Display(Name = "الوصف المختصر")]
	[StringLength(1024)]
	public string ShortDescription { get; set; }

	/// <inheritdoc />
	[Display(Name = "الوصف")]
	[StringLength(4096)]
	[DataType(DataType.MultilineText)]
	public string Description { get; set; }

	[Display(Name = "النوع")]
	public BuildingAdType AdType { get; set; }
	public string AdTypeName => AdType.GetDisplayName();

	[Display(Name = "الاولوية")]
	public byte? Priority { get; set; }

	[Display(Name = "التاريخ")]
	[Required]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime Date { get; set; }

	[Display(Name = "تاريخ الانتهاء")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime? Expires { get; set; }

	[Display(Name = "التليفون")]
	[Required]
	[Phone]
	[DataType(DataType.PhoneNumber)]
	public string Phone { get; set; }

	[Display(Name = "المحمول")]
	[Phone]
	[DataType(DataType.PhoneNumber)]
	public string Mobile { get; set; }

	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	[Required]
	[Range(0, 1000000000)]
	public long Price { get; set; }
}