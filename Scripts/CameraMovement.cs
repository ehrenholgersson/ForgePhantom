using Godot;
using System;
using System.Linq.Expressions;

public partial class CameraMovement : Camera3D
{
	[Export] Node3D player;
	[Export] Vector2 mouse_sensitivity;
	[Export] float zoom_speed;
	[Export] float max_zoom;
	[Export] float min_zoom;
	float camera_angle = 60;
	float camera_rotation = 0;
	float rotation_speed = 0.002f;
	float camera_distance = 10;
	float zoom_input;
	Vector2 last_mouse_position = Vector2.Zero;
	Vector2 mouse_velocity = Vector2.Zero;
	public Vector3 Camera_Forwards {get => ((GlobalTransform.Basis.Z.Project(Basis.Identity.Z))+(GlobalTransform.Basis.Z.Project(Basis.Identity.X))).Normalized(); }
	//public Vector3 Camera_Right {get => Camera_Forwards.Rotated(Basis.Identity.Y,90); }
	public float Camera_Rotation {get => camera_rotation; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	public void RotateCamera(float angle)
	{
		camera_rotation += angle;
	}
		public void PitchCamera(float angle)
	{
		camera_angle += angle;
	}

	public override void _UnhandledInput(InputEvent @event)
{
    if (@event is InputEventMouseMotion mouseEvent)
	{
        mouse_velocity = mouseEvent.Relative;
	}
	else if (@event is InputEventMouseButton mouseButton)
	{
		if (mouseButton.IsPressed())
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				zoom_input = 1;
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				zoom_input = -1;
			}
		}
	}
}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//float zoom = 0f;

		//take inputs to modify values for camera angle and rotation
		if (Input.IsActionPressed("camera_modifier"))
		{
			if (Input.IsActionJustPressed("camera_modifier"))
			{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			RotateCamera(mouse_velocity.X * (float)delta * mouse_sensitivity.X);
			PitchCamera(-mouse_velocity.Y * (float)delta * mouse_sensitivity.Y);
			//GD.Print ("Camera angle: "+ camera_angle);
			//GD.Print ("Camera rotation: "+ camera_rotation);
		}

		if (Input.IsActionJustReleased("camera_modifier"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		// take zoom inputs based on mouse wheel, doesn't work for some reason?
		//if (Input.IsMouseButtonPressed(MouseButton.WheelUp))
		//{	
		//	zoom = zoom_speed*-(float)delta;
		//}	
		//else if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
		//{
		//	zoom = zoom_speed*(float)delta;
		//}
		zoom_input *= (float)delta*zoom_speed;
		// update stored values
		camera_distance = Mathf.Clamp(camera_distance + zoom_input, min_zoom,max_zoom);
		camera_angle = Mathf.Clamp(camera_angle, 5f,85f);
		camera_rotation = camera_rotation % 360;
		// apply to camera position and rotation
		// the below works but feels jank
		GlobalPosition =player.Position + (new Vector3(0,Mathf.Tan(Mathf.DegToRad(camera_angle)),1) * Quaternion.FromEuler(new Vector3(0,Mathf.DegToRad(camera_rotation),0))).Normalized() * camera_distance; 
		LookAt(player.Position, Basis.Identity.Y,false);
		// reset mouse velocity value as this only updates when the mouse moves, should be zero whenever its not been updated
		 mouse_velocity = Vector2.Zero;
		 zoom_input = 0;
	}
}
