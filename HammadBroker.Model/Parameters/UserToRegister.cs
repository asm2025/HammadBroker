using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Parameters;

public class UserToRegister : UserToUpdate
{
    [Required]
    [StringLength(64, MinimumLength = 8)]
    public string Password { get; set; }
}