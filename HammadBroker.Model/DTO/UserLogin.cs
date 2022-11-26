using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

[Serializable]
public struct UserLogin
{
    [Required]
    [StringLength(128)]
    public string UserName { get; set; }

    [Required]
    [StringLength(64, MinimumLength = 8)]
    public string Password { get; set; }
}