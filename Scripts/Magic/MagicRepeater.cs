using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MagicRepeater : MagicNode
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
	}

	async void CheckRange()
	{
		while (this.IsInsideTree())
		{
			// check nodes
			await Task.Delay(5000);
		}
	}

	public int DrawPower(float request)
	{
		return 0;
	}

	public Dictionary<int, float> ServicedRequests()
	{
		return new Dictionary<int, float>();
	}
}
