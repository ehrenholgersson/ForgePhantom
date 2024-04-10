using Godot;
using System;

[GlobalClass]
public partial class CollectableResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh ObjectMesh;
    public float Health = 10;

    public Mesh Model { get => ObjectMesh; }

    public CollectableResource()
	{
        Icon = null;
        ObjectMesh = null;
        Health = 10;
	}
	public CollectableResource(CollectableResource original)
	{
        Icon = original.Icon;
        Health = original.Health;
        ObjectMesh = original.Model;
	}

	public CollectableResource newInstance()
	{
		return new CollectableResource((CollectableResource)this);
	}
}