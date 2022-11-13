using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Parameters;

public class UserToUpdate
{
    [Required]
    [StringLength(256)]
    public string FirstName { get; set; }

    [StringLength(256)]
    public string LastName { get; set; }

    [StringLength(256)]
    public string NickName { get; set; }

    [StringLength(320)]
    public string ImageUrl { get; set; }

    [Required]
    public Genders Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? CityId { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CountryCode { get; set; }

    [Required]
    [RegularExpression("^[a-zA-Z0-9_@\\-\\.\\+]+$")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool LockoutEnabled { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
}