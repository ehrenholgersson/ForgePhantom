using Godot;


[GlobalClass]
public partial class Collectable : RigidBody3D, IInteractable
{
	[Export] CollectableResource _item;
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
			AddChild(_meshInstance);
		}
		if (_item != null)
		{
			_meshInstance.Mesh = _item.ObjectMesh;
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
