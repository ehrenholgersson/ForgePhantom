using Godot;
using System;

[GlobalClass]
public partial class BuildingResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh ObjectMesh;
    [Export] public string BuildingName;
    public float Health = 10;

    public Mesh Model { get => ObjectMesh; }

    public BuildingResource()
    {
        BuildingName = string.Empty;
        Icon = null;
        ObjectMesh = null;
        Health = 10;
    }
    public BuildingResource(BuildingResource original)
    {
        Icon = original.Icon;
        Health = original.Health;
        ObjectMesh = original.Model;
        BuildingName = original.BuildingName;
    }

    public BuildingResource newInstance()
    {
        return new BuildingResource(this);
    }
}