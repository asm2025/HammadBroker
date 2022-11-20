using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Model.Entities;

[Index(nameof(Date))]
[Index(nameof(Expires))]
[Index(nameof(Price))]
public class BuildingAd : IEntity<int>
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int BuildingId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? Expires { get; set; }
    [Required]
    [Phone]
    public string Phone { get; set; }
    [Phone]
    public string Mobile { get; set; }
    [Required]
    public decimal Price { get; set; }
}