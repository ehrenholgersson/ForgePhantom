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
	GpuParticles3D _particles;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _entry = GetNode<Area3D>("Entry");
		_particles = GetNode<GpuParticles3D>("Particles");
		_entry.BodyEntered += (target) => PickupObject(target);
		if (DestinationNode != null)
		{
			_path = _destinationNode.GlobalPosition - GlobalPosition;
			SetupParticles();
		}
    }

	void SetupParticles()
	{
		if (_particles != null)
		{
			ParticleProcessMaterial process = _particles.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
			if (process != null)
			{
				process.EmissionShapeScale = Vector3.One * _pathradius;
				process.Direction = _path.Normalized();
				process.Spread = 0;
				process.InitialVelocityMax = _speed / process.ScaleMax;
                process.InitialVelocityMin = _speed / process.ScaleMax;
				_particles.ProcessMaterial = process;
				_particles.Lifetime = _path.Length() / (_speed / process.ScaleMax);
				_particles.Emitting = true;
            }
		}

	}

	void PickupObject(Node3D target)
	{
		if (target != null && target is Collectable && _destinationNode != null)
		{
			((Collectable)target).Transport(this);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
