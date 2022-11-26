using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class UserToRegister : UserToUpdateCore
{
    [Required]
    [StringLength(100, ErrorMessage = "{0} يجب الا تقل عن {2} وبحد أقصى {1} حرف أو رقم.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "تأكبد كلمة المرور")]
    [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور لايتطابقان.")]
    public string ConfirmPassword { get; set; }
}