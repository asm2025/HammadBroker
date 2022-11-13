using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using essentialMix.Extensions;
using Microsoft.AspNetCore.Identity;

namespace HammadBroker.Model.Entities;

public class ApplicationUser : IdentityUser<string>, IEntity<string>
{
    private string _firstName;
    private string _lastName;
    private string _nickName;

    /// <inheritdoc />
    public ApplicationUser()
    {
    }

    /// <inheritdoc />
    public ApplicationUser(string userName)
        : base(userName)
    {
    }


    [Required]
    [StringLength(256)]
    public string FirstName
    {
        get => _firstName;
        set => _firstName = value.ToNullIfEmpty();
    }

    [StringLength(256)]
    public string LastName
    {
        get => _lastName;
        set => _lastName = value.ToNullIfEmpty();
    }

    [StringLength(256)]
    public string NickName
    {
        get => _nickName ?? FirstName;
        set => _nickName = value.ToNullIfEmpty();
    }

    [StringLength(320)]
    public string ImageUrl { get; set; }
    public Genders Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? CityId { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CountryCode { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}