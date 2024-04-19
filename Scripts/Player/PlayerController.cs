using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	[Flags]public enum ControlMode 
	{
		Player = 1, 
		UI = 2, 
		Mixed = 4
	}

	[Export] CameraMovement _camera;
	bool _lockToCamera;
	[Export] ControlMode _enabledModes;

    public static ControlMode Control = ControlMode.Player;

    Area3D _surrounds;

	IInteractable _selectedObject;

	CollectableResource[] _inventory = new CollectableResource[9];
	Button[] _inButtons = new Button[9];
	
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public static Action OnInventoryUpdate;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        base._EnterTree();
		Control = ControlMode.Player;
		if ((int)_enabledModes == 0 || (int)_enabledModes > 7)
		{
			_enabledModes = (ControlMode)7;
		}
		while (!((_enabledModes & Control) == Control))
		{
            Control = (ControlMode)(((int)Control << 1) % 7);
		}
        GD.Print("Selected control scheme: "+Control);

        if (Control == ControlMode.Player)
		{
			_lockToCamera = true;
			Input.MouseMode = Input.MouseModeEnum.Captured; // -- test
		}
        _surrounds = GetNode<Area3D>("InteractionArea");
		int i = 0;
		foreach (Button b in GetNode("/root/Scene/Control/VBoxContainer/Inventory").GetChildren())
		{
			if (i<_inButtons.Length)
			{
				_inButtons[i] = b;
				i++;
			}
		}

		_surrounds.BodyEntered += (body) => UpdateObjectSelection(body);
		_surrounds.BodyExited += UpdateObjectSelection;
    }
    public bool PickUpObject(CollectableResource obj, int slot)
	{
        if (_inventory[slot] == null)
        {
            _inventory[slot] = obj;
            _inButtons[slot].Icon = obj.Icon;
            UpdateObjectSelection(true);
            OnInventoryUpdate?.Invoke();
            return true;
        }
		return false;
    }

    public bool PickUpObject(CollectableResource obj)
	{
		for (int i = 0; i < _inventory.Length;i++)
		{
			if (_inventory[i] == null)
			{
                // should be handling this in toolbar or inventory script when OnInventoryUpdate is called
                _inventory[i] = obj;
				_inButtons[i].Icon = obj.Icon;
				UpdateObjectSelection(true);
                OnInventoryUpdate?.Invoke();
                return true;
				
			}
		}
		return false;
	}

	public CollectableResource GetInventoryItem(int index)
	{
		return _inventory[index];
	}

	public void RemoveObject(int index)
	{
		_inventory[index] = null;
        OnInventoryUpdate?.Invoke();
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= Gravity * (float)delta;

		if (Input.IsActionJustPressed("control_mode"))
		{
			do
			{
				Control = (ControlMode)(((int)Control << 1) % 7);
			} while (!((_enabledModes & Control) == Control));

			if (Control == ControlMode.Player)
			{
                Input.MouseMode = Input.MouseModeEnum.Captured;
				_lockToCamera = true;
            }
			else
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
                _lockToCamera = false;
            }
            GD.Print("Selected control scheme: " + Control);
        }

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		if (Input.IsActionJustPressed("control_interaction"))
		{
			_selectedObject?.Interact();
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector3 camera_forward = _camera?.CameraForwards ?? Vector3.Forward;
		Vector2 inputDir = Input.GetVector("control_left","control_right","control_forward", "control_backward");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y).Normalized()).Rotated(Vector3.Up,-_camera.CameraRotation);
		if (direction != Vector3.Zero && IsOnFloor() && Control != ControlMode.UI)
		{
			velocity = (camera_forward * inputDir.Y + camera_forward.Rotated(Vector3.Up,1.57079633f)*inputDir.X)*Speed;
			if (Control == ControlMode.Mixed)
			{
				LookAt(GlobalPosition + velocity, Vector3.Up);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		if (_lockToCamera)
		{
			LookAt(GlobalPosition - camera_forward, Vector3.Up);
		}
        //GD.Print("Camera_Forwards: "+_camera.Camera_Forwards);
        Velocity = velocity;
		MoveAndSlide();
	}
    private void UpdateObjectSelection()
	{
		UpdateObjectSelection(false);
	}
    private void UpdateObjectSelection(Node3D body)
	{
        GD.Print(body.Name + " is in range");
        UpdateObjectSelection(false);
	}

    public void UpdateObjectSelection(bool ignoreCurrentSelection)
	{
		
		// clear selection
		IInteractable newSelection = null;
		// decide on new selection
		Godot.Collections.Array<Node3D> overlappers = _surrounds.GetOverlappingBodies();
		if (overlappers.Count > 0)
		{
			foreach (Node3D node in overlappers)
			{
				if (node is IInteractable && !node.IsQueuedForDeletion() && (!ignoreCurrentSelection||(node != _selectedObject)))// if we have just placed an object in our inventory, then the version of the object in the world will still actually exist at this point, so we should ignore it

                {
                    // need to add some weighting system here based on dot product of player facing vs direction to object as well as distance to object
                    newSelection = (IInteractable)node;
					
				}
			}

            if (newSelection != _selectedObject)
            {
				if (_selectedObject != null)
				{
					GetNode(((Node)_selectedObject).GetPath()+ "/InteractionBillboard")?.QueueFree();
				}
				_selectedObject = newSelection;
				if (newSelection != null)
				{
					WorldSpaceBillboard.New3DBillboard((Node3D)_selectedObject, Vector3.Up * 2f, "InteractionBillboard");
				}
			}
            GD.Print("Player interactable selected as " + (((Node)_selectedObject)?.Name ?? "nothing"));
        }
    }

// may need to create my own math helper class for this project
	public float signedangle (Vector2 a, Vector2 b)
	{
		return Mathf.Atan2(a.X*b.Y-b.X*a.Y,a.X*a.Y+b.X*b.Y);
	}

	public float angle360(Vector2 a, Vector2 b)
	{
		return (signedangle(a,b) + 360) % 360;
	}
}
