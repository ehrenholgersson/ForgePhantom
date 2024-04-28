using Godot;

/// <summary>
/// Building.cs handles most general building functions, but as thas is a resource and not a node class we need some node within the scene as a reference for each instance
/// </summary>

// seriously, what percentange of this script can just be the word building??!!
public partial class BuildingNode : Node3D
{
    Building _building;

    public Building Building { get => _building; }

    public BuildingNode (Building building)
    {
        _building = new Building(building);
        GameController.Singleton.AddChild(this);
        _building.TakeNodes(this);
    }
}
