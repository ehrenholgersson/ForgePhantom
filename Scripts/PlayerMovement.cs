using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] CameraMovement _camera;
	
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector3 camera_forward = _camera.Camera_Forwards;
		Vector2 inputDir = Input.GetVector("control_left","control_right","control_forward", "control_backward");
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y).Normalized()).Rotated(Vector3.Up,-_camera.Camera_Rotation);
		if (direction != Vector3.Zero)
		{
			velocity = (camera_forward * inputDir.Y + camera_forward.Rotated(Vector3.Up,1.57079633f)*inputDir.X)*Speed;
			//Rotation = new Vector3(0,angle360(new Vector2(direction.X, direction.Z),new Vector2 (0,1)),0);
			LookAt(GlobalPosition + velocity, Vector3.Up);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		
		//GD.Print("Camera_Forwards: "+_camera.Camera_Forwards);
		Velocity = velocity;
		MoveAndSlide();
	}

	public float signedangle (Vector2 a, Vector2 b)
	{
		return Mathf.Atan2(a.X*b.Y-b.X*a.Y,a.X*a.Y+b.X*b.Y);
	}

	public float angle360(Vector2 a, Vector2 b)
	{
		return (signedangle(a,b) + 360) % 360;
	}
}
