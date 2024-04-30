using Godot;
using System.Collections.Generic;

public partial class MagicSource : MagicNode
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		_powerLevel = 1;
		_powerSources.Add(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
