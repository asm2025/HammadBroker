using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace HammadBroker.Model.Entities;

public class Country : IEntity<string>
{
	private string _id;
	private string _name;

	[Key]
	[StringLength(3, MinimumLength = 3)]
	public string Id
	{
		get => _id;
		set => _id = value.ToNullIfEmpty()?.ToUpperInvariant();
	}

	[Required]
	[StringLength(255)]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}
}