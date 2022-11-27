using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Configuration;

public class CompanyInfo
{
    [Display(Name = "اسم الشركة")]
    public string Title { get; set; }
    [Display(Name = "الوصف")]
    public string Description { get; set; }
    [Display(Name = "البريد الالكتروني")]
    public string Email { get; set; }
    [Display(Name = "التليفون")]
    public string Phone { get; set; }
    [Display(Name = "المحمول")]
    public string Mobile { get; set; }
    [Display(Name = "العنوان")]
    public string Address { get; set; }
    [Display(Name = "العنوان 2")]
    public string Address2 { get; set; }
    [Display(Name = "الموقع الالكتروني")]
    public string Website { get; set; }
}