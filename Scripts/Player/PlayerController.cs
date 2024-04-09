using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class PlayerController : CharacterBody3D
{
	public enum ControlMode {player, UI }

	public static ControlMode controlMode = ControlMode.player;

	[Export] CameraMovement _camera;

	Area3D _surrounds;

	IInteractable selected_object;

	CollectableResource[] _inventory = new CollectableResource[9];
	Button[] _inButtons = new Button[9];
	
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public static Action OnInventoryUpdate;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        base._EnterTree();
        Input.MouseMode = Input.MouseModeEnum.Captured; // -- test
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
    }

	public bool PickUpObject(CollectableResource obj)
	{
		for (int i = 0; i < _inventory.Length;i++)
		{
			if (_inventory[i] == null)
			{
                // should be handling this in toolbar or inventory script when OnInventoryUpdate is called
                _inventory[i] = obj;
				_inButtons[i].Icon = obj.icon;
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
			velocity.Y -= gravity * (float)delta;

		if (Input.IsActionJustPressed("control_mode"))
		{
			if (controlMode == ControlMode.player)
			{
				controlMode = ControlMode.UI;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
			else
			{
                controlMode = ControlMode.player;
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		if (Input.IsActionJustPressed("control_interaction"))
		{
			selected_object?.Interact();
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector3 camera_forward = _camera?.Camera_Forwards ?? Vector3.Forward;
		Vector2 inputDir = Input.GetVector("control_left","control_right","control_forward", "control_backward");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y).Normalized()).Rotated(Vector3.Up,-_camera.Camera_Rotation);
		if (direction != Vector3.Zero && IsOnFloor() && controlMode== ControlMode.player)
		{
			velocity = (camera_forward * inputDir.Y + camera_forward.Rotated(Vector3.Up,1.57079633f)*inputDir.X)*Speed;
			//Rotation = new Vector3(0,angle360(new Vector2(direction.X, direction.Z),new Vector2 (0,1)),0);
			//LookAt(camera_forward,Vector3.Up);//(GlobalPosition + velocity, Vector3.Up);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		LookAt(GlobalPosition - camera_forward, Vector3.Up);
        //GD.Print("Camera_Forwards: "+_camera.Camera_Forwards);
        Velocity = velocity;
		MoveAndSlide();

		// check for interactions, there are better ways to do this (area entered/exited events), but it works for now
		
		if (_surrounds?.HasOverlappingBodies() ?? false)
		{
			List<Node3D> Overlappers = _surrounds.GetOverlappingBodies().ToList();
			// check if currently selected object still overlaps
			if (!Overlappers.Contains((Node3D)selected_object))
			{
				selected_object = null;
			}
			foreach (Node3D node in _surrounds.GetOverlappingBodies())
			{
				if (node is IInteractable)
				{
					// need to add some weighting system here based on dot product of player facing vs direction to object as well as distance to object
					selected_object = (IInteractable)node;
					GD.Print("Player interactable selected as "+ selected_object);
				}
			}
		}
		else
		{
			selected_object = null;
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
