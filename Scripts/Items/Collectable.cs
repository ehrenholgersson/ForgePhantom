using Godot;


[GlobalClass]
public partial class Collectable : RigidBody3D, IInteractable
{
	[Export] CollectableResource _item;
	MeshInstance3D _meshInstance;
	CollisionShape3D _collider; // - not sure if needed?
	RandomNumberGenerator _rng = new RandomNumberGenerator();

	TransportNode _transportNode;
	Vector3 _transportPath;
	bool _inTransit;

	WorldSpaceBillboard _interactionPrompt;

	public CollectableResource ItemType { get => _item; }

	
    //public Collectable(CollectableItem itm)
    //{
    //    item = itm;
    //}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		FreezeMode = FreezeModeEnum.Static;
		_item = _item.newInstance();; // create an instance of our item
		if (GetChildren().Count > 0)
		{
			_meshInstance = GetNode<MeshInstance3D>("Model");
		}
		if (_meshInstance == null)
		{
			_meshInstance = new MeshInstance3D();
			_meshInstance.Name = "Model";		
			AddChild(_meshInstance);
		}
		if (_item != null)
		{
			_meshInstance.Mesh = _item._model;
			_meshInstance.CreateConvexCollision(true,true);
            _collider = GetNode<CollisionShape3D>("Model/Model_col/CollisionShape3D");
			_collider.GetParent().RemoveChild(_collider);
			AddChild(_collider);
			GetNode("Model/Model_col")?.QueueFree();
		}
	}

	public void SetItem(CollectableResource itm)
	{
		_item = itm;
	}

	public void Interact()
	{
		if (GameController.MainPlayer.PickUpObject(_item))
		{
			QueueFree(); // destroy as player has picked us up
		}
	}

	public void Transport(TransportNode Origin)
	{
		_transportNode = Origin;
		_transportPath = Origin.TransportPath;
	}

	public void OnSelect()
	{
        _interactionPrompt = WorldSpaceBillboard.New3DBillboard(this, Vector3.Up * 2f, "InteractionBillboard");
    }

	public void OnDeselect()
	{
        _interactionPrompt?.QueueFree();

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_transportNode != null)
		{
			var traveled = (GlobalPosition - _transportNode.GlobalPosition); // distance and direction we have moved from the last transporter node

			//if ((_transportPath.Normalized() * traveled.Length()).DistanceTo(traveled) < _transportNode.PathRadius)
			if (traveled.Project(_transportPath.Normalized()).DistanceTo(traveled) < _transportNode.PathRadius && traveled.Length() < _transportPath.Length()) // todo - half decent physics
			{
				Freeze = true;
                _inTransit = true;
                GlobalPosition += _transportPath.Normalized() * (float)delta * _transportNode.Speed;
			}
			else
			{
				Freeze = false;
				_inTransit = false;
				_transportNode = null;
			}
		}
		else if (_inTransit)
		{
            Freeze = false;
            _inTransit = false;
        }
	}
}

