using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace HammadBroker.Model.Entities;

public class District : IEntity<int>
{
	private string _name;

	[Key]
	public int Id { get; set; }

	[Required]
	public int CityId { get; set; }

	[Required]
	[StringLength(256)]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}
}