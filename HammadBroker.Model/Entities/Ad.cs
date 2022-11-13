using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Model.Entities;

[Index(nameof(Date))]
[Index(nameof(BuildingId))]
public class Ad : IEntity<long>
{
    [Key]
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? Expires { get; set; }
    [Required]
    [Phone]
    public string Phone { get; set; }
    [Phone]
    public string Mobile { get; set; }
    [Required]
    public long BuildingId { get; set; }
    [Required]
    public decimal Price { get; set; }
}