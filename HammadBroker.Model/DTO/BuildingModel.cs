using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingModel
{
	private bool _canUpload;

	public BuildingModel([NotNull] IBuilding building)
	{
		Building = building;
	}

	public int Id { get; set; }

	[NotNull]
	public IBuilding Building { get; set; }

	public bool CanEdit { get; set; }

	public bool CanUpload
	{
		get => Id > 0 && _canUpload;
		set => _canUpload = value;
	}
}