using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum Genders
{
    [Display(Name = "غير محدد")]
    Unspecified,
    [Display(Name = "ذكر")]
    Male,
    [Display(Name = "انثى")]
    Female
}