using Godot;

// handling most general building functions in Building.cs, but as this is a resource and not a node class it needs some node within the scene to reference each instance
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
