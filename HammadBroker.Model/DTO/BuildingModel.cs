namespace HammadBroker.Model.DTO;

public class BuildingModel
{
	private bool _canEdit;

	public BuildingModel(IBuilding building)
	{
		Building = building;
	}

	public int Id { get; set; }

	public bool ReadOnly { get; set; }

	public bool CanEdit
	{
		get => !ReadOnly && _canEdit;
		set => _canEdit = value;
	}

	public bool CanUpload { get; set; }

	public IBuilding Building { get; }
}