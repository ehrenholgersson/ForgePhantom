using Godot;
using Godot.Collections;

public partial class new_script : Node3D, IMagicNode
{
	float _powerState;
	Magic.PowerTypes _powerType;
    public float PowerState { get => _powerState; }
    public Magic.PowerTypes PowerType { get => _powerType; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
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
