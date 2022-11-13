using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class FinishingType : IEntity<string>
{
	[Key]
	[StringLength(64)]
	public string Id { get; set; }
}