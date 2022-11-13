using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace HammadBroker.Model.Entities;

public class City : IEntity<int>
{
	private string _name;

	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(256)]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}

	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }
}
