using Godot;
using System;
using System.Linq.Expressions;

public partial class CameraMovement : Camera3D
{

	[Export] Node3D focus;
	[Export] Vector2 mouse_sensitivity;
	[Export] float zoomSpeed;
	[Export] bool invertMouse = true;
	[Export(PropertyHint.Range, "5,100,")] float maxZoom;
    [Export(PropertyHint.Range, "5,100,")] float minZoom;
	[Export(PropertyHint.Range, "5,85,")] float cameraAngle = 60;
	[Export(PropertyHint.Range, "0,359,")] float cameraRotation = 0;
	[Export(PropertyHint.Range, "5,100,")] float cameraDistance = 10;
	[Export] bool allowCameraRotation = true;	
	[Export] bool allowCameraTilt = true;
	[Export] bool allowZoom = true;
	float zoomInput;
	Vector2 lastMousePosition = Vector2.Zero;
	Vector2 mouseVelocity = Vector2.Zero;
	public Vector3 CameraForwards {get => (GlobalTransform.Basis.Z - GlobalTransform.Basis.Z.Project(Basis.Identity.Y)).Normalized();} // camera facing direction without any Y component - for player movement direction
	public float CameraRotation {get => cameraRotation; }
	int MouseInversion 
	{ 
		get 
		{
			if (invertMouse)
			{
				return -1;
			}
			return 1;
		}
	
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	public void RotateCamera(float angle)
	{
		cameraRotation += angle;
	}
		public void PitchCamera(float angle)
	{
		cameraAngle += angle;
	}

	public override void _Input(InputEvent @event)
{
    if (@event is InputEventMouseMotion mouseEvent)
	{
        mouseVelocity = mouseEvent.Relative;
	}
	else if (@event is InputEventMouseButton mouseButton)
	{
		if (mouseButton.IsPressed())
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				zoomInput = 1;
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				zoomInput = -1;
			}
		}
	}
}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (PlayerController.Control == PlayerController.ControlMode.Mixed)
		{

			if (Input.IsActionJustPressed("camera_modifier"))
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else if (Input.IsActionJustReleased("camera_modifier"))
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
		}

		//take inputs to modify values for camera angle and rotation
		if (PlayerController.Control==PlayerController.ControlMode.Player || (PlayerController.Control == PlayerController.ControlMode.Mixed && Input.IsActionPressed("camera_modifier")))
		{

			if (allowCameraRotation)
			{
				RotateCamera(mouseVelocity.X * (float)delta * mouse_sensitivity.X);
			}
			if (allowCameraTilt)
			{
				PitchCamera(mouseVelocity.Y * (float)delta * mouse_sensitivity.Y* MouseInversion);
			}
		}
		
		// update stored values
		if (allowZoom)
		{
            zoomInput *= (float)delta * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance + zoomInput, minZoom,maxZoom);
		}
		cameraAngle = Mathf.Clamp(cameraAngle, 5f,85f);
		cameraRotation = cameraRotation % 360;
		
		// apply to camera position and rotation
		// the below works but feels slightly jank
		GlobalPosition = focus?.Position + (new Vector3(0,Mathf.Tan(Mathf.DegToRad(cameraAngle)),1) * Quaternion.FromEuler(new Vector3(0,Mathf.DegToRad(cameraRotation),0))).Normalized() * cameraDistance ?? GlobalPosition; 
		LookAt(focus?.Position ?? Vector3.Zero, Basis.Identity.Y,false);
		// reset mouse velocity value as this only updates when the mouse moves, should be zero whenever its not updating
		 mouseVelocity = Vector2.Zero;
		 zoomInput = 0;
	}
}
