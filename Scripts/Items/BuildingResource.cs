using Godot;
using System;

[GlobalClass]
public partial class BuildingResource : Resource, IDraggable
{
    [Export] public Texture2D icon;
    [Export] public Mesh model;
    public float health = 10;

    public Mesh Model { get => model; }

    public BuildingResource()
    {
        icon = null;
        model = null;
        health = 10;
    }
    public BuildingResource(BuildingResource original)
    {
        icon = original.icon;
        health = original.health;
        model = original.model;
    }

    public BuildingResource newInstance()
    {
        return new BuildingResource((BuildingResource)this);
    }
}