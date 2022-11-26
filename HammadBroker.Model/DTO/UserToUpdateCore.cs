using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class UserToUpdateCore
{
    [Required]
    [EmailAddress]
    [Display(Name = "البريد الالكتروني")]
    public string Email { get; set; }

    [Phone]
    [Display(Name = "رقم التليفون")]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(256)]
    [Display(Name = "الاسم الأول")]
    public string FirstName { get; set; }

    [StringLength(256)]
    [Display(Name = "الاسم الأخير")]
    public string LastName { get; set; }

    [StringLength(256)]
    [Display(Name = "الكنية")]
    public string NickName { get; set; }

    [StringLength(320)]
    [Display(Name = "الصورة")]
    public string ImageUrl { get; set; }

    [Required]
    [Display(Name = "النوع")]
    public Genders Gender { get; set; }
    [Display(Name = "تاريخ الميلاد")]
    public DateTime? DateOfBirth { get; set; }
}