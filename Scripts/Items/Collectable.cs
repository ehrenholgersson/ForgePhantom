using Godot;


[GlobalClass]
public partial class Collectable : RigidBody3D, IInteractable
{
	[Export] CollectableInfo _item;
	MeshInstance3D _meshInstance;
	CollisionShape3D _collider; // - not sure if needed?
	RandomNumberGenerator _rng = new RandomNumberGenerator();
    //public Collectable(CollectableItem itm)
    //{
    //    item = itm;
    //}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_item = _item.newInstance();; // create an instance of our item
		if (GetChildren().Count > 0)
		{
			_meshInstance = GetNode<MeshInstance3D>("Model");
		}
		if (_meshInstance == null)
		{
			_meshInstance = new MeshInstance3D();
			_meshInstance.Name = "Model";
			GD.Print(_meshInstance.Name);
			
			AddChild(_meshInstance);
		}
		if (_item != null)
		{
			_meshInstance.Mesh = _item.model;
			_meshInstance.CreateConvexCollision(true,true);
            GD.Print(_meshInstance.Name + '/' + _meshInstance.GetChild(0).Name + '/'+ _meshInstance.GetChild(0).GetChild(0).Name);
            _collider = GetNode<CollisionShape3D>("Model/Model_col/CollisionShape3D");
			_collider.GetParent().RemoveChild(_collider);
			AddChild(_collider);
			GetNode("Model/Model_col")?.QueueFree();
		}
		GD.Print("Health is "+_item.health);
		_item.health = _rng.RandiRange(0, 30);
	}

	public void SetItem(CollectableInfo itm)
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}

