using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class Building : IEntity<long>
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Title { get; set; }

    [Required]
    [StringLength(64)]
    public string BuildingType { get; set; }

    [Required]
    [StringLength(64)]
    public string FinishingType { get; set; }

    [Required]
    [StringLength(64)]
    public string Floor { get; set; }

    [StringLength(2048)]
    public string Location { get; set; }

    [StringLength(1024)]
    public string Address { get; set; }

    [StringLength(1024)]
    public string Address2 { get; set; }

    public int? CityId { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CountryCode { get; set; }

    [Required]
    [StringLength(2048)]
    public string Description { get; set; }
}