using Godot;
using System;

[GlobalClass]
public partial class CollectableInfo : Resource
{
	[Export] public Texture2D icon;
	[Export] public Mesh model;
	public float health = 10;

	public CollectableInfo()
	{
		icon = null;
		model = null;
		health = 10;
	}
	public CollectableInfo(CollectableInfo original)
	{
		icon = original.icon;
		health = original.health;
		model = original.model;
	}

	public CollectableInfo newInstance()
	{
		return new CollectableInfo((CollectableInfo)this);
	}
}