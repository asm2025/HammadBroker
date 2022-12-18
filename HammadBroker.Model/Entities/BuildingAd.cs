using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class BuildingAd : IEntity<int>
{
	[Key]
	public int Id { get; set; }
	public BuildingAdType Type { get; set; }
	public byte Priority { get; set; }
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
	public long Price { get; set; }
	public long Views { get; set; }
	public long PageViews { get; set; }
	public long Requests { get; set; }
}