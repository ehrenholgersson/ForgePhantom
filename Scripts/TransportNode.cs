using Godot;
using System;

public partial class TransportNode : Node3D
{

	[Export] TransportNode _destinationNode;
	Vector3 _path;
    [Export] float _pathradius;
	[Export] float _speed;
	public TransportNode DestinationNode { get => _destinationNode; }
	public Vector3 TransportPath { get => _path; }
	public float PathRadius { get => _pathradius; }
	public float Speed { get => _speed; }
	Area3D _entry;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _entry = GetNode<Area3D>("Entry");
		_entry.BodyEntered += (target) => PickupObject(target);
		if (DestinationNode != null)
		{
			_path = _destinationNode.GlobalPosition - GlobalPosition;
		}
    }

	void PickupObject(Node3D target)
	{
		if (target != null ||target is Collectable && _destinationNode != null)
		{
			((Collectable)target).Transport(this);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
