using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Model.Entities;

[Index(nameof(BuildingType))]
[Index(nameof(FinishingType))]
[Index(nameof(Floor))]
[Index(nameof(CityId))]
[Index(nameof(CountryCode))]
public class Building : IEntity<long>
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Title { get; set; }

    [Required]
    public BuildingType BuildingType { get; set; }

    [Required]
    public FinishingType FinishingType { get; set; }

    [Required]
    [StringLength(32)]
    public string Floor { get; set; }

    [StringLength(2048)]
    public string Location { get; set; }

    [StringLength(512)]
    public string Address { get; set; }

    [StringLength(512)]
    public string Address2 { get; set; }

    public int? CityId { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CountryCode { get; set; }

    [Required]
    [StringLength(2048)]
    public string Description { get; set; }
}