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
		var parent = GetParent();

        if (parent is BuildingNode)
		{
			((BuildingNode)parent).Building.OnNodeSet += (dest) => SetDestination(dest);
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
				process.Direction = _path.Normalized() * GlobalBasis.GetRotationQuaternion();
				process.Spread = 0;
				process.InitialVelocityMax = _speed / process.ScaleMax;
                process.InitialVelocityMin = _speed / process.ScaleMax;
				_particles.ProcessMaterial = process;
				_particles.Lifetime = _path.Length() / (_speed / process.ScaleMax);
				_particles.Emitting = true;
            }
		}

	}

	void SetDestination(Node3D destination)
	{
		GD.Print("setting destination node to " + destination.Name);
		var target = destination.GetNode("Transporter");
		if (target is TransportNode && target != this)
		{
			_destinationNode = target as TransportNode;
            _path = _destinationNode.GlobalPosition - GlobalPosition;
            SetupParticles();
        }
    }

	void PickupObject(Node3D target)
	{
		GD.Print("moving " + target.Name);
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
