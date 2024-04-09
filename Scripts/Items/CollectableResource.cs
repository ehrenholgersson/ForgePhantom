using Godot;
using System;

[GlobalClass]
public partial class CollectableResource : Resource, IDraggable
{
	[Export] public Texture2D icon;
	[Export] public Mesh model;
	public float health = 10;

    public Mesh Model { get => model; }

    public CollectableResource()
	{
		icon = null;
		model = null;
		health = 10;
	}
	public CollectableResource(CollectableResource original)
	{
		icon = original.icon;
		health = original.health;
		model = original.model;
	}

	public CollectableResource newInstance()
	{
		return new CollectableResource((CollectableResource)this);
	}
}