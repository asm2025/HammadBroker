using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Configuration;

public class CompanyInfo
{
	private string _whatsAppNumber;

	[Display(Name = "الشركة")]
	public string Name { get; set; }
	[Display(Name = "الوصف")]
	public string Description { get; set; }
	[Display(Name = "البريد الالكتروني")]
	public string Email { get; set; }
	[Display(Name = "التليفون")]
	public string Phone { get; set; }
	[Display(Name = "المحمول")]
	public string Mobile { get; set; }

	[Display(Name = "واتس آب")]
	public string WhatsAppNumber
	{
		get => _whatsAppNumber;
		set => _whatsAppNumber = value?.Trim('+', '0', ' ');
	}

	[Display(Name = "العنوان")]
	public string Address { get; set; }
	[Display(Name = "الحي")]
	public string Address2 { get; set; }
	[Display(Name = "المدينة")]
	public string City { get; set; }
	[Display(Name = "الموقع الالكتروني")]
	public string Website { get; set; }
}